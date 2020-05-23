using System.Linq;
using SlugDB;
public class PersonTable : Table<Person>
{
    public static Person Ben => Table<Person>.Instance.rows.SingleOrDefault(p => p.prettyName == "Ben");
}
