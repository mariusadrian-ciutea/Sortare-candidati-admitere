import { sql } from "drizzle-orm";
import { index, integer, sqliteTable, text } from "drizzle-orm/sqlite-core";

export const applications = sqliteTable(
  "applications",
  {
    id: integer("id").primaryKey({ autoIncrement: true }),
    submissionCode: text("submission_code").notNull().unique(),
    nume: text("nume").notNull(),
    prenume: text("prenume").notNull(),
    adresa: text("adresa").notNull(),
    varsta: integer("varsta").notNull(),
    sex: text("sex").notNull(),
    cnp: text("cnp").notNull().unique(),
    email: text("email").notNull(),
    telefon: text("telefon").notNull(),
    medieBac: integer("medie_bac_x100").notNull(),
    medieLiceu: integer("medie_liceu_x100").notNull(),
    optionsJson: text("options_json").notNull(),
    status: text("status").notNull().default("pending"),
    createdAt: text("created_at").notNull().default(sql`CURRENT_TIMESTAMP`),
    importedAt: text("imported_at"),
  },
  (table) => [
    index("applications_status_idx").on(table.status),
    index("applications_created_at_idx").on(table.createdAt),
  ],
);
