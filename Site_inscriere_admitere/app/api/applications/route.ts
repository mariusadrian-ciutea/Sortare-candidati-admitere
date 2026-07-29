import { env } from "cloudflare:workers";
import { ensureSchema } from "../../../db/ensure-schema";
import { validSpecializations } from "../../../lib/catalog";

type ApplicationPayload = {
  nume?: string;
  prenume?: string;
  adresa?: string;
  varsta?: number;
  sex?: string;
  cnp?: string;
  email?: string;
  telefon?: string;
  medieBac?: number;
  medieLiceu?: number;
  options?: string[];
  consent?: boolean;
};

function clean(value: unknown) {
  return typeof value === "string" ? value.trim() : "";
}

function error(message: string, status = 400) {
  return Response.json({ error: message }, { status });
}

export async function POST(request: Request) {
  try {
    await ensureSchema();
    const payload = (await request.json()) as ApplicationPayload;
    const nume = clean(payload.nume);
    const prenume = clean(payload.prenume);
    const adresa = clean(payload.adresa);
    const sex = clean(payload.sex);
    const cnp = clean(payload.cnp);
    const email = clean(payload.email).toLowerCase();
    const telefon = clean(payload.telefon);
    const varsta = Number(payload.varsta);
    const medieBac = Number(payload.medieBac);
    const medieLiceu = Number(payload.medieLiceu);
    const options = Array.isArray(payload.options)
      ? payload.options.map(clean).filter(Boolean)
      : [];

    if (!nume || !prenume || /\d/.test(nume + prenume))
      return error("Numele și prenumele trebuie completate corect.");
    if (!adresa) return error("Adresa este obligatorie.");
    if (!Number.isInteger(varsta) || varsta < 16 || varsta > 100)
      return error("Vârsta introdusă nu este validă.");
    if (!["Masculin", "Feminin"].includes(sex))
      return error("Selectează sexul.");
    if (!/^\d{13}$/.test(cnp))
      return error("CNP-ul trebuie să conțină exact 13 cifre.");
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
      return error("Adresa de e-mail nu este validă.");
    if (!/^[+0-9 ()-]{8,20}$/.test(telefon))
      return error("Numărul de telefon nu este valid.");
    if (
      !Number.isFinite(medieBac) ||
      !Number.isFinite(medieLiceu) ||
      medieBac < 1 ||
      medieBac > 10 ||
      medieLiceu < 1 ||
      medieLiceu > 10
    )
      return error("Mediile trebuie să fie între 1 și 10.");
    if (options.length < 1 || options.length > 3)
      return error("Selectează între una și trei opțiuni.");
    if (new Set(options).size !== options.length)
      return error("Aceeași specializare nu poate fi selectată de două ori.");
    if (options.some((option) => !validSpecializations.has(option as never)))
      return error("Una dintre specializări nu este disponibilă.");
    if (payload.consent !== true)
      return error("Acordul pentru prelucrarea datelor este obligatoriu.");

    const submissionCode = `ADM-${crypto.randomUUID()
      .slice(0, 8)
      .toUpperCase()}`;

    await env.DB.prepare(
      `INSERT INTO applications (
        submission_code, nume, prenume, adresa, varsta, sex, cnp,
        email, telefon, medie_bac_x100, medie_liceu_x100, options_json
      ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
    )
      .bind(
        submissionCode,
        nume,
        prenume,
        adresa,
        varsta,
        sex,
        cnp,
        email,
        telefon,
        Math.round(medieBac * 100),
        Math.round(medieLiceu * 100),
        JSON.stringify(options),
      )
      .run();

    return Response.json({ submissionCode }, { status: 201 });
  } catch (caught) {
    const message = caught instanceof Error ? caught.message : "Eroare necunoscută";
    if (message.includes("UNIQUE") && message.includes("cnp"))
      return error("Există deja o înscriere pentru acest CNP.", 409);
    return error("Înscrierea nu a putut fi salvată. Încearcă din nou.", 500);
  }
}
