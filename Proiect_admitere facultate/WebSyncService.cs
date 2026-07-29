using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Proiect_admitere_facultate
{
    internal sealed class WebSubmission
    {
        public int id { get; set; }
        public string submissionCode { get; set; }
        public string nume { get; set; }
        public string prenume { get; set; }
        public string adresa { get; set; }
        public int varsta { get; set; }
        public string sex { get; set; }
        public string cnp { get; set; }
        public double medieBac { get; set; }
        public double medieLiceu { get; set; }
        public List<string> options { get; set; }
        public string createdAt { get; set; }
    }

    internal sealed class PendingSubmissionsResponse
    {
        public List<WebSubmission> submissions { get; set; }
    }

    internal sealed class SyncResult
    {
        public int Received { get; set; }
        public int Imported { get; set; }
        public int AlreadyPresent { get; set; }
        public int Failed { get; set; }
    }

    internal static class WebSyncService
    {
        public static bool IsConfigured
        {
            get
            {
                string url = ConfigurationManager.AppSettings["WebApiBaseUrl"];
                string token = ConfigurationManager.AppSettings["WebImportToken"];
                return !string.IsNullOrWhiteSpace(url) &&
                       !url.StartsWith("__", StringComparison.Ordinal) &&
                       !string.IsNullOrWhiteSpace(token) &&
                       !token.StartsWith("__", StringComparison.Ordinal);
            }
        }

        public static async Task<SyncResult> SynchronizeAsync()
        {
            string baseUrl = ConfigurationManager.AppSettings["WebApiBaseUrl"];
            string token = ConfigurationManager.AppSettings["WebImportToken"];
            if (!IsConfigured)
                throw new InvalidOperationException(
                    "Preluarea înscrierilor nu a fost configurată încă.");

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await client.GetAsync("api/admin/submissions");
                string json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(
                        "Serverul formularului a răspuns cu o eroare: " + json);

                PendingSubmissionsResponse payload =
                    serializer.Deserialize<PendingSubmissionsResponse>(json);
                List<WebSubmission> submissions =
                    payload != null && payload.submissions != null
                        ? payload.submissions
                        : new List<WebSubmission>();

                SyncResult result = new SyncResult { Received = submissions.Count };
                List<int> confirmedIds = new List<int>();

                foreach (WebSubmission submission in submissions)
                {
                    try
                    {
                        ImportResult importResult =
                            DatabaseManager.ImportWebSubmission(submission);
                        if (importResult == ImportResult.Imported)
                            result.Imported++;
                        else
                            result.AlreadyPresent++;
                        confirmedIds.Add(submission.id);
                    }
                    catch
                    {
                        // Înregistrarea rămâne online cu status pending și poate fi reîncercată.
                        result.Failed++;
                    }
                }

                if (confirmedIds.Count > 0)
                {
                    string confirmationJson =
                        serializer.Serialize(new { ids = confirmedIds });
                    using (StringContent content = new StringContent(
                        confirmationJson, Encoding.UTF8, "application/json"))
                    {
                        HttpResponseMessage confirmation =
                            await client.PostAsync("api/admin/submissions", content);
                        string confirmationBody =
                            await confirmation.Content.ReadAsStringAsync();
                        if (!confirmation.IsSuccessStatusCode)
                            throw new InvalidOperationException(
                                "Datele au fost importate, dar confirmarea a eșuat: " +
                                confirmationBody);
                    }
                }

                return result;
            }
        }
    }
}
