using HueTour.CoreBusiness;
using HueTour.CoreBusiness.BusinessRules;
using HueTour.UseCases.PluginInterfaces.State;

namespace HueTour.Plugins.DataStore.InMemory;

public class BookingCartStateService : IBookingCartStateService
{
    public event Action? OnChange;

    public int SelectedTourId => SelectedTour?.Id ?? 0;
    public Tour? SelectedTour { get; private set; }
    public Departure? SelectedDeparture { get; private set; }
    public List<PassengerTicket> TempPassengers { get; private set; } = new();
    public Customer TempCustomer { get; private set; } = new();
    public decimal CurrentTotalAmount { get; private set; }

    public void SelectTourAndDeparture(Tour tour, Departure departure)
    {
        SelectedTour = tour;
        SelectedDeparture = departure;

        if (!TempPassengers.Any())
        {
            // Mặc định 1 hành khách người lớn
            TempPassengers.Add(new PassengerTicket
            {
                FullName = string.Empty,
                Gender = "Nam",
                PassengerType = "Adult",
                BirthYear = DateTime.Now.Year - 25,
                Price = tour.BasePrice
            });
        }

        NotifyStateChanged();
    }

    public void AddPassenger(PassengerTicket ticket)
    {
        TempPassengers.Add(ticket);
        NotifyStateChanged();
    }

    public void RemovePassengerAt(int index)
    {
        if (index >= 0 && index < TempPassengers.Count)
        {
            TempPassengers.RemoveAt(index);
            NotifyStateChanged();
        }
    }

    public void UpdatePassenger(int index, PassengerTicket ticket)
    {
        if (index >= 0 && index < TempPassengers.Count)
        {
            TempPassengers[index] = ticket;
            NotifyStateChanged();
        }
    }

    public void SetCustomer(Customer customer)
    {
        TempCustomer = customer;
        NotifyStateChanged();
    }

    public void RecalculateTotal(IEnumerable<PriceRule> priceRules)
    {
        if (SelectedTour == null)
        {
            CurrentTotalAmount = 0;
            return;
        }

        decimal total = 0;
        foreach (var p in TempPassengers)
        {
            p.Price = BookingBusinessRules.CalculatePassengerFare(SelectedTour.BasePrice, p.PassengerType, priceRules);
            total += p.Price;
        }

        CurrentTotalAmount = total;
        NotifyStateChanged();
    }

    public void Clear()
    {
        SelectedTour = null;
        SelectedDeparture = null;
        TempPassengers.Clear();
        TempCustomer = new Customer();
        CurrentTotalAmount = 0;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
