using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AutoService.Data;

namespace AutoService.Pages
{
    public partial class VisitListWindow : Window
    {
        public VisitListWindow(Client selectedClient)
        {
            InitializeComponent();

            // Устанавливаем имя клиента в заголовке
            ClientNameTextBlock.Text = $"Посещения клиента: {selectedClient.FirstName} {selectedClient.LastName}";

            // Загружаем посещения клиента
            LoadVisits(selectedClient);
        }

        private void LoadVisits(Client client)
        {
            using (var context = new Connect())
            {
                // Получаем список посещений
                var visits = context.ClientService
                    .Where(cs => cs.ClientID == client.ID)
                    .Select(cs => new
                    {
                        VisitDate = cs.StartTime,
                        Comment = cs.Comment,
                        FileCount = cs.DocumentByService.Count 
                    })
                    .ToList();

                VisitsDataGrid.ItemsSource = visits;
            }
        }
    }
}
