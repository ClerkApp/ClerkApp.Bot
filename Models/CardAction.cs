namespace ClerkBot.Models
{
    public interface ICardAction<T>
    {
        public T Action { get; set; }
    }

    public class CardAction<T>: ICardAction<T>
    {
        public T Action { get; set; }
    }
}
