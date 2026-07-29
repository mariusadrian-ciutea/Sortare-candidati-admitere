import { env } from "cloudflare:workers";
import { ensureSchema } from "../../../../db/ensure-schema";

type RuntimeEnv = typeof env & { ADMISSION_IMPORT_TOKEN?: string };

function authorized(request: Request) {
  const configured = (env as RuntimeEnv).ADMISSION_IMPORT_TOKEN;
  const supplied = request.headers.get("authorization");
  return Boolean(configured && supplied === `Bearer ${configured}`);
}

export async function GET(request: Request) {
  if (!authorized(request))
    return Response.json({ error: "Unauthorized" }, { status: 401 });

  await ensureSchema();
  const result = await env.DB.prepare(
    `SELECT
      id,
      submission_code AS submissionCode,
      nume,
      prenume,
      adresa,
      varsta,
      sex,
      cnp,
      medie_bac_x100 AS medieBacX100,
      medie_liceu_x100 AS medieLiceuX100,
      options_json AS optionsJson,
      created_at AS createdAt
    FROM applications
    WHERE status = 'pending'
    ORDER BY id
    LIMIT 200`,
  ).all<{
    id: number;
    submissionCode: string;
    nume: string;
    prenume: string;
    adresa: string;
    varsta: number;
    sex: string;
    cnp: string;
    medieBacX100: number;
    medieLiceuX100: number;
    optionsJson: string;
    createdAt: string;
  }>();

  const submissions = result.results.map((row) => ({
    id: row.id,
    submissionCode: row.submissionCode,
    nume: row.nume,
    prenume: row.prenume,
    adresa: row.adresa,
    varsta: row.varsta,
    sex: row.sex,
    cnp: row.cnp,
    medieBac: row.medieBacX100 / 100,
    medieLiceu: row.medieLiceuX100 / 100,
    options: JSON.parse(row.optionsJson) as string[],
    createdAt: row.createdAt,
  }));

  return Response.json({ submissions });
}

export async function POST(request: Request) {
  if (!authorized(request))
    return Response.json({ error: "Unauthorized" }, { status: 401 });

  await ensureSchema();
  const payload = (await request.json()) as { ids?: number[] };
  const ids = Array.isArray(payload.ids)
    ? [...new Set(payload.ids.filter((id) => Number.isInteger(id) && id > 0))]
    : [];
  if (!ids.length)
    return Response.json({ error: "No valid ids" }, { status: 400 });

  await env.DB.batch(
    ids.map((id) =>
      env.DB.prepare(
        `UPDATE applications
         SET status = 'imported', imported_at = CURRENT_TIMESTAMP
         WHERE id = ? AND status = 'pending'`,
      ).bind(id),
    ),
  );
  return Response.json({ confirmed: ids.length });
}
