import { env } from "cloudflare:workers";

const createApplications = `
  CREATE TABLE IF NOT EXISTS applications (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    submission_code TEXT NOT NULL UNIQUE,
    nume TEXT NOT NULL,
    prenume TEXT NOT NULL,
    adresa TEXT NOT NULL,
    varsta INTEGER NOT NULL,
    sex TEXT NOT NULL,
    cnp TEXT NOT NULL UNIQUE,
    email TEXT NOT NULL,
    telefon TEXT NOT NULL,
    medie_bac_x100 INTEGER NOT NULL,
    medie_liceu_x100 INTEGER NOT NULL,
    options_json TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'pending',
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    imported_at TEXT
  )
`;

const createStatusIndex =
  "CREATE INDEX IF NOT EXISTS applications_status_idx ON applications(status)";

let initialized = false;

export async function ensureSchema() {
  if (initialized) return;
  await env.DB.batch([
    env.DB.prepare(createApplications),
    env.DB.prepare(createStatusIndex),
  ]);
  initialized = true;
}
