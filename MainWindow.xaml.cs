using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace ShopifyNotifierWPF
{
    public partial class MainWindow : Window
    {
        private const string Subdomain = "lafamiliale"; // Sous-domaine fixe LocalTunnel
        private const int Port = 5000;                  // Port pour serveur intégré
        private Process ltProcess;

        public ObservableCollection<Order> Orders { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Orders = new ObservableCollection<Order>();
            DataGridOrders.ItemsSource = Orders;

            // URL publique
            LabelServerUrl.Text = $"https://{Subdomain}.loca.lt";
            LabelWebhookUrl.Text = $"https://{Subdomain}.loca.lt/webhook";

            // Lancer LocalTunnel automatiquement
            StartLocalTunnel();

            // Lancer le serveur ASP.NET Core intégré
            StartWebhookServer();
        }

        #region LocalTunnel automatique
        private void StartLocalTunnel()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C lt --port {Port} --subdomain {Subdomain}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            ltProcess = new Process();
            ltProcess.StartInfo = startInfo;

            ltProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Dispatcher.Invoke(() =>
                    {
                        LabelServerUrl.Text = $"https://{Subdomain}.loca.lt";
                        LabelWebhookUrl.Text = $"https://{Subdomain}.loca.lt/webhook";
                        LabelStatus.Text = "LocalTunnel actif";
                    });
                }
            };

            ltProcess.Start();
            ltProcess.BeginOutputReadLine();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Fermer LocalTunnel
            if (ltProcess != null && !ltProcess.HasExited)
            {
                ltProcess.Kill();
                ltProcess.Dispose();
            }
        }
        #endregion

        #region Serveur ASP.NET Core intégré
        private void StartWebhookServer()
        {
            Task.Run(() =>
            {
                var builder = WebApplication.CreateBuilder();
                var app = builder.Build();

                app.MapPost("/webhook", async context =>
                {
                    using var reader = new StreamReader(context.Request.Body);
                    var json = await reader.ReadToEndAsync();
                    ShopifyOrder order = null;

                    try
                    {
                        order = JsonSerializer.Deserialize<ShopifyOrder>(json);
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                            MessageBox.Show("Erreur JSON webhook : " + ex.Message)
                        );
                    }

                    if (order != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Orders.Add(new Order
                            {
                                Id = order.id,
                                Client = $"{order.customer?.first_name} {order.customer?.last_name}".Trim(),
                                Total = order.total_price
                            });

                            // Ouvre le pop-up
                            var popup = new OrderPopup(order);
                            popup.Show();
                        });
                    }

                    await context.Response.WriteAsync("OK");
                });

                app.Run($"http://localhost:{Port}");
            });
        }
        #endregion

        #region Boutons UI
        private void BtnCopyServerUrl_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText($"https://{Subdomain}.loca.lt");
            MessageBox.Show("Adresse du serveur copiée !");
        }

        private void BtnCopyWebhookUrl_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText($"https://{Subdomain}.loca.lt/webhook");
            MessageBox.Show("URL du webhook copiée !");
        }
        #endregion

        #region Classes internes
        public class Order
        {
            public long Id { get; set; }
            public string Client { get; set; }
            public string Total { get; set; }
        }

       
        #endregion
    }
}
