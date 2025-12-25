
namespace FlashcardsPlatformFull.Services;

public static class Srs
{
    // SM-2 inspired scheduling:
    // grade: 1=Again, 2=Hard, 3=Good, 5=Easy
    public static (double ef, int reps, int intervalDays) Apply(int grade, double ef, int reps, int intervalDays)
    {
        if (grade < 3)
        {
            reps = 0;
            intervalDays = 1;
        }
        else
        {
            reps += 1;
            intervalDays = reps switch
            {
                1 => 1,
                2 => 6,
                _ => (int)Math.Round(intervalDays * ef)
            };

            // EF update
            ef += (0.1 - (5 - grade) * (0.08 + (5 - grade) * 0.02));
            if (ef < 1.3) ef = 1.3;
        }

        if (intervalDays < 1) intervalDays = 1;
        return (ef, reps, intervalDays);
    }
}
