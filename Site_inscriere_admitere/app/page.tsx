"use client";

import { FormEvent, useState } from "react";
import { facultyCatalog } from "../lib/catalog";

type FormData = {
  nume: string;
  prenume: string;
  cnp: string;
  adresa: string;
  varsta: string;
  sex: string;
  email: string;
  telefon: string;
  medieBac: string;
  medieLiceu: string;
  consent: boolean;
};

const initialForm: FormData = {
  nume: "",
  prenume: "",
  cnp: "",
  adresa: "",
  varsta: "",
  sex: "",
  email: "",
  telefon: "",
  medieBac: "",
  medieLiceu: "",
  consent: false,
};

const steps = [
  { number: 1, label: "Date personale" },
  { number: 2, label: "Opțiuni" },
  { number: 3, label: "Confirmare" },
];

export default function Home() {
  const [step, setStep] = useState(1);
  const [form, setForm] = useState<FormData>(initialForm);
  const [facultyIndex, setFacultyIndex] = useState(0);
  const [specialization, setSpecialization] = useState(
    facultyCatalog[0].specializations[0] as string,
  );
  const [options, setOptions] = useState<string[]>([]);
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [submissionCode, setSubmissionCode] = useState("");

  function updateField(
    field: keyof FormData,
    value: string | boolean,
  ) {
    setForm((current) => ({ ...current, [field]: value }));
    setError("");
  }

  function validatePersonal() {
    if (!form.nume.trim() || !form.prenume.trim())
      return "Completează numele și prenumele.";
    if (/\d/.test(form.nume + form.prenume))
      return "Numele și prenumele nu pot conține cifre.";
    if (!/^\d{13}$/.test(form.cnp))
      return "CNP-ul trebuie să conțină exact 13 cifre.";
    if (!form.adresa.trim()) return "Completează adresa de domiciliu.";
    const age = Number(form.varsta);
    if (!Number.isInteger(age) || age < 16 || age > 100)
      return "Introdu o vârstă validă.";
    if (!form.sex) return "Selectează sexul.";
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
      return "Introdu o adresă de e-mail validă.";
    if (!/^[+0-9 ()-]{8,20}$/.test(form.telefon))
      return "Introdu un număr de telefon valid.";
    const bac = Number(form.medieBac.replace(",", "."));
    const highSchool = Number(form.medieLiceu.replace(",", "."));
    if (
      !Number.isFinite(bac) ||
      !Number.isFinite(highSchool) ||
      bac < 1 ||
      bac > 10 ||
      highSchool < 1 ||
      highSchool > 10
    )
      return "Mediile trebuie să fie numere între 1 și 10.";
    return "";
  }

  function goToOptions() {
    const validationError = validatePersonal();
    if (validationError) {
      setError(validationError);
      return;
    }
    setError("");
    setStep(2);
    window.scrollTo({ top: 0 });
  }

  function addOption() {
    if (options.length >= 3) {
      setError("Poți adăuga maximum trei opțiuni.");
      return;
    }
    if (options.includes(specialization)) {
      setError("Această specializare este deja în listă.");
      return;
    }
    setOptions((current) => [...current, specialization]);
    setError("");
  }

  function removeOption(index: number) {
    setOptions((current) => current.filter((_, itemIndex) => itemIndex !== index));
  }

  function goToReview() {
    if (!options.length) {
      setError("Adaugă cel puțin o opțiune.");
      return;
    }
    setError("");
    setStep(3);
    window.scrollTo({ top: 0 });
  }

  async function submitApplication(event: FormEvent) {
    event.preventDefault();
    if (!form.consent) {
      setError("Confirmă acordul pentru prelucrarea datelor.");
      return;
    }
    setSubmitting(true);
    setError("");
    try {
      const response = await fetch("/api/applications", {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({
          ...form,
          varsta: Number(form.varsta),
          medieBac: Number(form.medieBac.replace(",", ".")),
          medieLiceu: Number(form.medieLiceu.replace(",", ".")),
          options,
        }),
      });
      const payload = (await response.json()) as {
        submissionCode?: string;
        error?: string;
      };
      if (!response.ok)
        throw new Error(payload.error || "Înscrierea nu a putut fi trimisă.");
      setSubmissionCode(payload.submissionCode || "");
      setStep(4);
      window.scrollTo({ top: 0 });
    } catch (caught) {
      setError(
        caught instanceof Error
          ? caught.message
          : "A apărut o eroare. Încearcă din nou.",
      );
    } finally {
      setSubmitting(false);
    }
  }

  const selectedFaculty = facultyCatalog[facultyIndex];

  return (
    <main className="site-shell">
      <div className="aero-backdrop" aria-hidden="true" />

      <header className="topbar">
        <div className="brand" aria-label="Admitere Online">
          <span>
            <strong>Admitere online</strong>
            <small>Sesiunea 2026</small>
          </span>
        </div>
      </header>

      <section className="hero">
        <div className="hero-copy">
          <p className="eyebrow">ÎNSCRIERE</p>
          <h1>Formular de admitere</h1>
          <p className="hero-lead">
            Completează datele, alege opțiunile în ordinea preferată și trimite
            înscrierea.
          </p>
        </div>

        <div className="application-frame">
          <aside className="step-rail" aria-label="Progres formular">
            <p className="step-caption">PROGRESUL TĂU</p>
            <ol>
              {steps.map((item) => (
                <li
                  key={item.number}
                  className={
                    step === item.number
                      ? "active"
                      : step > item.number
                        ? "complete"
                        : ""
                  }
                >
                  <span>{step > item.number ? "✓" : item.number}</span>
                  <div>
                    <small>Pasul {item.number}</small>
                    <strong>{item.label}</strong>
                  </div>
                </li>
              ))}
            </ol>
          </aside>

          <section className="form-card" aria-live="polite">
            {step === 1 && (
              <>
                <div className="form-heading">
                  <div>
                    <p>Pasul 1 din 3</p>
                    <h2>Datele candidatului</h2>
                  </div>
                </div>

                <div className="form-grid">
                  <Field label="Nume" required>
                    <input
                      value={form.nume}
                      onChange={(event) => updateField("nume", event.target.value)}
                      autoComplete="family-name"
                      placeholder="ex. Popescu"
                    />
                  </Field>
                  <Field label="Prenume" required>
                    <input
                      value={form.prenume}
                      onChange={(event) =>
                        updateField("prenume", event.target.value)
                      }
                      autoComplete="given-name"
                      placeholder="ex. Andrei"
                    />
                  </Field>
                  <Field label="CNP" required hint="13 cifre">
                    <input
                      value={form.cnp}
                      onChange={(event) =>
                        updateField(
                          "cnp",
                          event.target.value.replace(/\D/g, "").slice(0, 13),
                        )
                      }
                      inputMode="numeric"
                      autoComplete="off"
                      placeholder="0000000000000"
                    />
                  </Field>
                  <Field label="Vârstă" required>
                    <input
                      value={form.varsta}
                      onChange={(event) =>
                        updateField(
                          "varsta",
                          event.target.value.replace(/\D/g, "").slice(0, 3),
                        )
                      }
                      inputMode="numeric"
                      placeholder="18"
                    />
                  </Field>
                  <Field label="Adresa de domiciliu" required wide>
                    <input
                      value={form.adresa}
                      onChange={(event) =>
                        updateField("adresa", event.target.value)
                      }
                      autoComplete="street-address"
                      placeholder="Stradă, număr, localitate, județ"
                    />
                  </Field>
                  <Field label="E-mail" required>
                    <input
                      value={form.email}
                      onChange={(event) =>
                        updateField("email", event.target.value)
                      }
                      type="email"
                      autoComplete="email"
                      placeholder="nume@email.ro"
                    />
                  </Field>
                  <Field label="Telefon" required>
                    <input
                      value={form.telefon}
                      onChange={(event) =>
                        updateField("telefon", event.target.value)
                      }
                      type="tel"
                      autoComplete="tel"
                      placeholder="+40 7xx xxx xxx"
                    />
                  </Field>
                  <Field label="Sex" required>
                    <select
                      value={form.sex}
                      onChange={(event) => updateField("sex", event.target.value)}
                    >
                      <option value="">Selectează</option>
                      <option>Feminin</option>
                      <option>Masculin</option>
                    </select>
                  </Field>
                  <div className="average-fields">
                    <Field label="Media BAC" required>
                      <input
                        value={form.medieBac}
                        onChange={(event) =>
                          updateField("medieBac", event.target.value)
                        }
                        inputMode="decimal"
                        placeholder="9,50"
                      />
                    </Field>
                    <Field label="Media liceului" required>
                      <input
                        value={form.medieLiceu}
                        onChange={(event) =>
                          updateField("medieLiceu", event.target.value)
                        }
                        inputMode="decimal"
                        placeholder="9,20"
                      />
                    </Field>
                  </div>
                </div>
                <ErrorMessage message={error} />
                <div className="form-actions align-end">
                  <button className="button primary" onClick={goToOptions}>
                    Continuă la opțiuni <span>→</span>
                  </button>
                </div>
              </>
            )}

            {step === 2 && (
              <>
                <div className="form-heading">
                  <div>
                    <p>Pasul 2 din 3</p>
                    <h2>Ordinea preferințelor</h2>
                  </div>
                  <span className="count-chip">{options.length}/3 opțiuni</span>
                </div>
                <p className="section-intro">
                  Prima opțiune are cea mai mare prioritate. Poți adăuga
                  maximum trei specializări.
                </p>

                <div className="choice-builder">
                  <Field label="Facultate">
                    <select
                      value={facultyIndex}
                      onChange={(event) => {
                        const index = Number(event.target.value);
                        setFacultyIndex(index);
                        setSpecialization(
                          facultyCatalog[index].specializations[0] as string,
                        );
                      }}
                    >
                      {facultyCatalog.map((entry, index) => (
                        <option key={entry.short} value={index}>
                          {entry.faculty}
                        </option>
                      ))}
                    </select>
                  </Field>
                  <Field label="Specializare">
                    <select
                      value={specialization}
                      onChange={(event) => setSpecialization(event.target.value)}
                    >
                      {selectedFaculty.specializations.map((item) => (
                        <option key={item}>{item}</option>
                      ))}
                    </select>
                  </Field>
                  <button
                    className="button add-button"
                    type="button"
                    onClick={addOption}
                  >
                    <span>＋</span> Adaugă în listă
                  </button>
                </div>

                <div className="preference-list">
                  {options.length === 0 ? (
                    <div className="empty-state">
                      <span>1—3</span>
                      <div>
                        <strong>Lista este goală</strong>
                        <p>Alege facultatea și specializarea de mai sus.</p>
                      </div>
                    </div>
                  ) : (
                    options.map((option, index) => (
                      <article className="preference-item" key={option}>
                        <span className="priority">{index + 1}</span>
                        <div>
                          <small>Prioritatea {index + 1}</small>
                          <strong>{option}</strong>
                        </div>
                        <button
                          className="remove-button"
                          type="button"
                          onClick={() => removeOption(index)}
                          aria-label={`Elimină ${option}`}
                        >
                          Elimină
                        </button>
                      </article>
                    ))
                  )}
                </div>
                <ErrorMessage message={error} />
                <div className="form-actions">
                  <button className="button secondary" onClick={() => setStep(1)}>
                    <span>←</span> Înapoi
                  </button>
                  <button className="button primary" onClick={goToReview}>
                    Verifică înscrierea <span>→</span>
                  </button>
                </div>
              </>
            )}

            {step === 3 && (
              <form onSubmit={submitApplication}>
                <div className="form-heading">
                  <div>
                    <p>Pasul 3 din 3</p>
                    <h2>Verifică și confirmă</h2>
                  </div>
                  <span className="review-chip">Ultimul pas</span>
                </div>

                <div className="review-grid">
                  <ReviewCard title="Date candidat" onEdit={() => setStep(1)}>
                    <strong>
                      {form.nume} {form.prenume}
                    </strong>
                    <p>CNP •••••••••{form.cnp.slice(-4)}</p>
                    <p>{form.email}</p>
                    <p>{form.telefon}</p>
                  </ReviewCard>
                  <ReviewCard title="Medii" onEdit={() => setStep(1)}>
                    <div className="score-row">
                      <span>BAC <strong>{form.medieBac}</strong></span>
                      <span>Liceu <strong>{form.medieLiceu}</strong></span>
                    </div>
                  </ReviewCard>
                  <ReviewCard title="Opțiuni" onEdit={() => setStep(2)} wide>
                    <ol className="review-options">
                      {options.map((option) => (
                        <li key={option}>{option}</li>
                      ))}
                    </ol>
                  </ReviewCard>
                </div>

                <label className="consent-box">
                  <input
                    type="checkbox"
                    checked={form.consent}
                    onChange={(event) =>
                      updateField("consent", event.target.checked)
                    }
                  />
                  <span>
                    Confirm că datele sunt corecte și sunt de acord cu
                    prelucrarea lor exclusiv în scopul procesului de admitere.
                  </span>
                </label>
                <ErrorMessage message={error} />
                <div className="form-actions">
                  <button
                    className="button secondary"
                    type="button"
                    onClick={() => setStep(2)}
                  >
                    <span>←</span> Înapoi
                  </button>
                  <button
                    className="button primary submit"
                    type="submit"
                    disabled={submitting}
                  >
                    {submitting ? "Se trimite…" : "Trimite înscrierea"}
                    {!submitting && <span>✓</span>}
                  </button>
                </div>
              </form>
            )}

            {step === 4 && (
              <div className="success-state">
                <div className="success-mark">✓</div>
                <p className="eyebrow">ÎNSCRIERE ÎNREGISTRATĂ</p>
                <h2>Înscriere trimisă.</h2>
                <p>
                  Păstrează codul de mai jos pentru identificarea înscrierii.
                </p>
                <div className="submission-code">
                  <small>CODUL ÎNSCRIERII</small>
                  <strong>{submissionCode}</strong>
                </div>
                <button
                  className="button secondary"
                  onClick={() => {
                    setForm(initialForm);
                    setOptions([]);
                    setSubmissionCode("");
                    setStep(1);
                  }}
                >
                  Începe o înscriere nouă
                </button>
              </div>
            )}
          </section>
        </div>
      </section>
    </main>
  );
}

function Field({
  label,
  hint,
  required,
  wide,
  children,
}: {
  label: string;
  hint?: string;
  required?: boolean;
  wide?: boolean;
  children: React.ReactNode;
}) {
  return (
    <label className={`field ${wide ? "wide" : ""}`}>
      <span>
        {label} {required && <b>*</b>}
        {hint && <small>{hint}</small>}
      </span>
      {children}
    </label>
  );
}

function ErrorMessage({ message }: { message: string }) {
  if (!message) return null;
  return (
    <div className="error-message" role="alert">
      <span>!</span>
      {message}
    </div>
  );
}

function ReviewCard({
  title,
  onEdit,
  wide,
  children,
}: {
  title: string;
  onEdit: () => void;
  wide?: boolean;
  children: React.ReactNode;
}) {
  return (
    <article className={`review-card ${wide ? "wide" : ""}`}>
      <header>
        <span>{title}</span>
        <button type="button" onClick={onEdit}>
          Editează
        </button>
      </header>
      {children}
    </article>
  );
}
