import assert from "node:assert/strict";
import { access, readFile } from "node:fs/promises";
import test from "node:test";

const root = new URL("../", import.meta.url);

test("keeps the registration page focused on the form", async () => {
  const page = await readFile(new URL("app/page.tsx", root), "utf8");

  assert.match(page, /<h1>Formular de admitere<\/h1>/);
  assert.match(page, /Datele candidatului/);
  assert.match(page, /Ordinea preferințelor/);
  assert.match(page, /Trimite înscrierea/);

  assert.doesNotMatch(page, /Formula de admitere|70%|Medie estimată/);
  assert.doesNotMatch(page, /Formular securizat|în siguranță|Date protejate/);
  assert.doesNotMatch(page, /className="brand-mark"|href="#"/);
  assert.doesNotMatch(page, /behavior:\s*"smooth"/);
});

test("uses the mature aero background asset", async () => {
  const css = await readFile(new URL("app/globals.css", root), "utf8");

  assert.match(css, /url\("\/aero-background\.png"\)/);
  assert.doesNotMatch(css, /\.sun|\.cloud|\.hill|\.bubble/);
  await access(new URL("public/aero-background.png", root));
});
