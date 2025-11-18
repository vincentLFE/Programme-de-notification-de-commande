using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Media;
using System.Windows;

namespace ShopifyNotifierWPF
{
    public partial class OrderPopup : Window
    {
        private SoundPlayer player;

        public OrderPopup(ShopifyOrder order)
        {
            InitializeComponent();

            decimal total = 0;
            decimal.TryParse(order.total_price, NumberStyles.Any, CultureInfo.InvariantCulture, out total);

            // Titre de la popup
            TextOrderInfo.Text = $"🎉 Nouvelle commande #{order.id} 🎉";

            // Informations client
            AddDetail("Client :", $"{order.customer?.first_name} {order.customer?.last_name}");
            AddDetail("Email :", order.customer?.email);
            AddDetail("Téléphone :", order.customer?.phone);

            // Somme totale
            AddDetail("Prix total :", $"{total:0.00} €");

            // Date et heure
            AddDetail("Date :", order.created_at);
            AddDetail("Heure de ramassage :", order.pickup_time);

            // Commentaire
            AddDetail("Commentaire :", string.IsNullOrWhiteSpace(order.note) ? "Aucun" : order.note);

            // Adresse de livraison
            if (order.shipping_address != null)
            {
                AddDetail("Adresse :", $"{order.shipping_address.address1}, {order.shipping_address.city}, {order.shipping_address.zip}, {order.shipping_address.country}");
            }

            // Liste des produits commandés
            if (order.line_items != null && order.line_items.Count > 0)
            {
                AddDetail("Produits commandés :", ""); // titre pour la section
                foreach (var item in order.line_items)
                {
                    AddDetail($"- {item.Name}", $"Quantité : {item.Quantity}, Prix : {item.Price} €");
                }
            }

            // Jouer le son d'alerte
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "alert.wav");
            if (File.Exists(path))
            {
                player = new SoundPlayer(path);
                player.PlayLooping();
            }
            else
            {
                MessageBox.Show("Le fichier son alert.wav est introuvable dans Assets !");
            }
        }

        private void AddDetail(string label, string value)
        {
            var tb = new System.Windows.Controls.TextBlock
            {
                Text = $"{label} {value}",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 5, 0, 5),
                TextWrapping = TextWrapping.Wrap
            };
            StackOrderDetails.Children.Add(tb);
        }

        private void BtnValidate_Click(object sender, RoutedEventArgs e)
        {
            if (player != null)
            {
                player.Stop();
                player.Dispose();
            }
            this.Close();
        }
    }
}
