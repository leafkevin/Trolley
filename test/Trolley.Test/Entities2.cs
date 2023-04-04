using System;

namespace Trolley.Test;

public class Person
{
    public static Person[] Rockstars = new[]
    {
        new Person(1, "Jimi", "Hendrix", 27),
        new Person(2, "Janis", "Joplin", 27),
        new Person(3, "Jim", "Morrisson", 27),
        new Person(4, "Kurt", "Cobain", 27),
        new Person(5, "Elvis", "Presley", 42),
        new Person(6, "Michael", "Jackson", 50),
    };
    [Key(IsIdentity = true)]
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }

    public Person() { }
    public Person(int id, string firstName, string lastName, int age)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Age = age;
    }
    protected bool Equals(Person other)
    {
        return Id == other.Id &&
            string.Equals(FirstName, other.FirstName) &&
            string.Equals(LastName, other.LastName) &&
            Age == other.Age;
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Person)obj);
    }
    public override int GetHashCode()
        => HashCode.Combine(this.Id, this.FirstName, this.LastName, this.Age);
}
public class PersonWithAutoId
{
    [Key(IsIdentity = true)]
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}
public class PersonWithNullableAutoId
{
    [Key(IsIdentity = true)]
    public int? Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}
public class EntityWithId
{
    [Key(IsIdentity = true)]
    public int Id { get; set; }
}
public class PersonWithAliasedAge
{
    [Key]
    public string Name { get; set; }
    [Field(FieldName = "YearsOld")]
    public int Age { get; set; }
    public string Ignored { get; set; }
}
public class PersonUsingEnumAsInt
{
    public string Name { get; set; }
    public Gender Gender { get; set; }
}
public class PersonWithReferenceType
{
    [Key(IsIdentity = true)]
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    [Reference]
    public Person BestFriend { get; set; }

    public static PersonWithReferenceType[] TestValues = new[]
    {
        new PersonWithReferenceType
        {
            FirstName = "Test",
            LastName = "McTest",
            Id = 1
        },
        new PersonWithReferenceType
        {
            FirstName = "John",
            LastName = "Doe",
            Id = 2,
            BestFriend = new Person(1,"Jane","Doe",33)
        }
    };

    protected bool Equals(PersonWithReferenceType other)
    {
        return Id == other.Id &&
            string.Equals(FirstName, other.FirstName) &&
            string.Equals(LastName, other.LastName) &&
            ((BestFriend == null && other.BestFriend == null) || (BestFriend != null && BestFriend.Equals(other.BestFriend)));
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((PersonWithReferenceType)obj);
    }
    public override int GetHashCode()
        => HashCode.Combine(this.Id, this.FirstName, this.LastName, this.BestFriend);
}
public class TestProduct
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime? Modified { get; set; }

    public static TestProduct[] TestValues =
    {
        new TestProduct
        {
            Id = "1",
            Modified = null,
            Name = "Testing"
        }
    };

    protected bool Equals(TestProduct other)
    {
        return Id == other.Id &&
            string.Equals(Name, other.Name) &&
            Modified.Equals(other.Modified);
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TestProduct)obj);
    }
    public override int GetHashCode()
        => HashCode.Combine(this.Id, this.Name, this.Modified);
}