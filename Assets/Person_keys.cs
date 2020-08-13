using System.Linq;
using SlugDB;

public class PersonTable : Table<Person>
{
    public static Person Marge => Find("Marge", false);
    public static Person Homer => Find("Homer", false);
    public static Person Bart => Find("Bart", false);
    public static Person Lisa => Find("Lisa", false);
    public static Person Maggie => Find("Maggie", false);
    public static Person Moe => Find("Moe", false);
    public static Person Barney => Find("Barney", false);
    public static Person Krusty => Find("Krusty", false);
}
