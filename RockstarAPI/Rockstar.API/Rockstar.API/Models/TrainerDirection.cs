namespace Rockstar.API.Models
{
    public class TrainerDirection
    {
        public int TrainerId { get; set; }
        public int DirectionId { get; set; }

        // Навигационные свойства
        public Trainer Trainer { get; set; } = null!;
        public Direction Direction { get; set; } = null!;
    }
}