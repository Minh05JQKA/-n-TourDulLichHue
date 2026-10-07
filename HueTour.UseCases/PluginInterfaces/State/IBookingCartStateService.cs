using HueTour.CoreBusiness;

namespace HueTour.UseCases.PluginInterfaces.State;

public interface IBookingCartStateService
{
    event Action? OnChange;

    int SelectedTourId { get; }
    Tour? SelectedTour { get; }
    Departure? SelectedDeparture { get; }
    List<PassengerTicket> TempPassengers { get; }
    Customer TempCustomer { get; }
    decimal CurrentTotalAmount { get; }

    void SelectTourAndDeparture(Tour tour, Departure departure);
    void AddPassenger(PassengerTicket ticket);
    void RemovePassengerAt(int index);
    void UpdatePassenger(int index, PassengerTicket ticket);
    void SetCustomer(Customer customer);
    void RecalculateTotal(IEnumerable<PriceRule> priceRules);
    void Clear();
}
