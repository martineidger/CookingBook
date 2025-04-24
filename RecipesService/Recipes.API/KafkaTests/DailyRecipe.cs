namespace Recipes.API.KafkaTests
{
    public class DailyRecipe
    {
        public string Id {  get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
    }
}
