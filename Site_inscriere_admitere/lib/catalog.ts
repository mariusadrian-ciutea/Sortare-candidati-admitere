export const facultyCatalog = [
  {
    faculty: "Facultatea de Cibernetică, Statistică și Informatică Economică",
    short: "CSIE",
    specializations: [
      "Cibernetică Economică",
      "Informatică Economică",
      "Statică economică și data science",
    ],
  },
  {
    faculty: "Facultatea de Management",
    short: "MAN",
    specializations: ["Management", "Management (în limba engleză)"],
  },
  {
    faculty: "Facultatea de Contabilitate și Informatică de Gestiune",
    short: "CIG",
    specializations: [
      "Contabilitate și Informatică de Gestiune",
      "Contabilitate și Informatică de Gestiune (în limba engleză)",
    ],
  },
  {
    faculty: "Facultatea de Marketing",
    short: "MK",
    specializations: ["Marketing", "Marketing (în limba engleză)"],
  },
  {
    faculty: "Facultatea de Finanțe, Asigurări, Bănci și Burse de Valori",
    short: "FABBV",
    specializations: [
      "Finanțe și Bănci",
      "Finanțe și Bănci (în limba engleză)",
    ],
  },
  {
    faculty: "Facultatea de Relații Economice Internaționale",
    short: "REI",
    specializations: [
      "Economie și afaceri internaționale",
      "Economie și afaceri internaționale (în limba engleză)",
      "Limbi moderne aplicate (engleză, franceză)",
    ],
  },
  {
    faculty: "Facultatea de Economie Teoretică și Aplicată",
    short: "ETA",
    specializations: ["Economie și comunicare economică în afaceri"],
  },
] as const;

export const validSpecializations = new Set(
  facultyCatalog.flatMap((entry) => [...entry.specializations]),
);
