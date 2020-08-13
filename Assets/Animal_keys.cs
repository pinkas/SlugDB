using System.Linq;
using SlugDB;

public class AnimalTable : Table<Animal>
{
    public static Animal Lion => Find("Lion", false);
    public static Animal Monkey => Find("Monkey", false);
    public static Animal Platypus => Find("Platypus", false);
    public static Animal Cat => Find("Cat", false);
}
