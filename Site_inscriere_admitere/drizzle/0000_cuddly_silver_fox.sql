CREATE TABLE `applications` (
	`id` integer PRIMARY KEY AUTOINCREMENT NOT NULL,
	`submission_code` text NOT NULL,
	`nume` text NOT NULL,
	`prenume` text NOT NULL,
	`adresa` text NOT NULL,
	`varsta` integer NOT NULL,
	`sex` text NOT NULL,
	`cnp` text NOT NULL,
	`email` text NOT NULL,
	`telefon` text NOT NULL,
	`medie_bac_x100` integer NOT NULL,
	`medie_liceu_x100` integer NOT NULL,
	`options_json` text NOT NULL,
	`status` text DEFAULT 'pending' NOT NULL,
	`created_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL,
	`imported_at` text
);
--> statement-breakpoint
CREATE UNIQUE INDEX `applications_submission_code_unique` ON `applications` (`submission_code`);--> statement-breakpoint
CREATE UNIQUE INDEX `applications_cnp_unique` ON `applications` (`cnp`);--> statement-breakpoint
CREATE INDEX `applications_status_idx` ON `applications` (`status`);--> statement-breakpoint
CREATE INDEX `applications_created_at_idx` ON `applications` (`created_at`);