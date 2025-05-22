# How to use
1. Inport `UnityAttributes.dll` to your unity project
2. Disable all ckeckboxes in `UnityAttributes.dll` import settings panel. Press `Apply` <img width="531" alt="image" src="https://github.com/user-attachments/assets/a95b96d5-2c1e-40c7-94a4-82e5984b696d">
3. Add `RoslynAnalyzer` tag to `UnityAttributes.dll` <img width="537" alt="image" src="https://github.com/user-attachments/assets/241a5db9-26c1-4f5d-ba10-958c5cdee314">

---

## Attribute Usage Guide

### 1. `Alias`

**Purpose:**  
Generates a class with constructors, `ToString` , `Equals` , and other utility methods, acting as an alias for another type.

**Usage Example:**

```csharp
[Alias(forType: typeof(SomeOtherType))]
public partial class MyAlias
{
    private int _value1;
    private string _value2;
}
```

This will generate constructors and utility methods for `MyAlias` based on its fields.

---

### 2. `GenConstructor`

**Purpose:**  
Automatically generates constructors for a class, including all non-static fields except those marked with `GenConstructorIgnore` .

**Usage Example:**

```csharp
[GenConstructor]
public partial class MyClass
{
    private int _value1, _value2;
    public int value3;
    [GenConstructorIgnore] private int _ignored;
    private readonly int _readonlyValue;
}
```

This will generate a constructor accepting `_value1` , `_value2` , `value3` , and `_readonlyValue` .

---

### 3. `MonoReadonly`

**Purpose:**  
Restricts assignment to the field only within Unity MonoBehaviour lifecycle methods: `Awake` , `OnEnable` , `Start` , or `Reset` . Assigning elsewhere will trigger a Roslyn analyzer error.

**Usage Example:**

```csharp
public class MyComponent : MonoBehaviour
{
    [MonoReadonly]
    private int _score;

    void Awake()
    {
        _score = 10; // Allowed
    }

    void Update()
    {
        _score = 20; // Error: not allowed
    }
}
```

---

### 4. `PublicAccessor`

**Purpose:**  
Generates a public property accessor for a private or protected field.

**Usage Example:**

```csharp
public partial class MyClass
{
    [PublicAccessor] private int _value;
}
```

This will generate:

```csharp
public int Value => _value;
```

---

### 5. `Readonly`

**Purpose:**  
Prevents assignment to the field outside of its declaration (enforced by analyzer). Useful for enforcing immutability in Unity projects.

**Usage Example:**

```csharp
public class MyClass
{
    [Readonly]
    private int _score = 10;

    void SomeMethod()
    {
        // _score = 20; // Error: assignment not allowed
    }
}
```

---

### 6. `Record`

**Purpose:**  
Generates a record-like class with value equality, `ToString` , and other utility methods, similar to C# 9 `record` but for older C# or Unity compatibility.

**Usage Example:**

```csharp
[Record]
public partial class MyRecord
{
    public int Id;
    public string Name;
}
```

This will generate constructors, `Equals` , `GetHashCode` , and `ToString` for `MyRecord` .

---

### 7. `Singleton`

**Purpose:**  
Implements the Unity MonoBehaviour singleton pattern. The static `Instance` is set in the specified initialization method (e.g., `Awake` ).

**Usage Example:**

```csharp
[Singleton("Awake")]
public partial class GameManager : MonoBehaviour
{
    // GameManager.Instance will be set in Awake()
}
```

---

## Generated Code Examples

Below you can see what code you write, what code is generated, and how to use the generated code for each attribute.

### 1. `Alias`

**You write:**

```csharp
[Alias(forType: typeof(SomeOtherType))]
public partial class MyAlias
{
    private int _value1;
    private string _value2;
}
```

**Generated code:**

```csharp
public partial class MyAlias
{
    public MyAlias() { }

    public MyAlias(int value1, string value2)
    {
        this._value1 = value1;
        this._value2 = value2;
    }

    public override string ToString() => $"MyAlias(_value1: {_value1}, _value2: {_value2})";

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = 0;
            hashCode = (hashCode * 397) ^ _value1.GetHashCode();
            hashCode = (hashCode * 397) ^ (_value2?.GetHashCode() ?? 0);
            return hashCode;
        }
    }

    public bool Equals(MyAlias other)
        => _value1.Equals(other._value1) && _value2 == other._value2;

    public override bool Equals(object obj)
        => obj is MyAlias other && Equals(other);

    public static bool operator ==(MyAlias left, MyAlias right) => left.Equals(right);
    public static bool operator !=(MyAlias left, MyAlias right) => !left.Equals(right);
}
```

**How to use:**

```csharp
var alias = new MyAlias(42, "hello");
Console.WriteLine(alias); // MyAlias(_value1: 42, _value2: hello)
```

---

### 2. `GenConstructor`

**You write:**

```csharp
[GenConstructor]
public partial class MyClass
{
    private int _value1, _value2;
    public int value3;
    [GenConstructorIgnore] private int _ignored;
    private readonly int _readonlyValue;
}
```

**Generated code:**

```csharp
public partial class MyClass
{
    public MyClass() { }

    public MyClass(int value1, int value2, int value3, int readonlyValue)
    {
        this._value1 = value1;
        this._value2 = value2;
        this.value3 = value3;
        this._readonlyValue = readonlyValue;
    }
}
```

**How to use:**

```csharp
var obj = new MyClass(1, 2, 3, 4);
```

---

### 3. `MonoReadonly`

**You write:**

```csharp
public class MyComponent : MonoBehaviour
{
    [MonoReadonly]
    private int _score;

    void Awake() { _score = 10; } // Allowed
    void Update() { _score = 20; } // Error: not allowed
}
```

**Generated code:**  
_No code is generated, but a Roslyn analyzer will enforce assignment rules._

**How to use:**  
Assign to the field only in `Awake` , `OnEnable` , `Start` , or `Reset` .  
Any assignment elsewhere will cause a compile-time error.

---

### 4. `PublicAccessor`

**You write:**

```csharp
public partial class MyClass
{
    [PublicAccessor] private int _value;
}
```

**Generated code:**

```csharp
public int Value => _value;
```

**How to use:**

```csharp
var obj = new MyClass();
int v = obj.Value; // Accesses the private _value field
```

---

### 5. `Readonly`

**You write:**

```csharp
public class MyClass
{
    [Readonly]
    private int _score = 10;
}
```

**Generated code:**  
_No code is generated, but a Roslyn analyzer will prevent assignment outside of the declaration._

**How to use:**  
You can only assign to `_score` at its declaration.  
Any assignment elsewhere will cause a compile-time error.

---

### 6. `Record`

**You write:**

```csharp
[Record]
public partial class MyRecord
{
    public int Id;
    public string Name;
}
```

**Generated code:**

```csharp
public partial class MyRecord
{
    public MyRecord() { }

    public MyRecord(int id, string name)
    {
        this.Id = id;
        this.Name = name;
    }

    public override string ToString() => $"MyRecord(Id: {Id}, Name: {Name})";

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = 0;
            hashCode = (hashCode * 397) ^ Id.GetHashCode();
            hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
            return hashCode;
        }
    }

    public bool Equals(MyRecord other)
        => Id.Equals(other.Id) && Name == other.Name;

    public override bool Equals(object obj)
        => obj is MyRecord other && Equals(other);

    public static bool operator ==(MyRecord left, MyRecord right) => left.Equals(right);
    public static bool operator !=(MyRecord left, MyRecord right) => !left.Equals(right);
}
```

**How to use:**

```csharp
var rec = new MyRecord(1, "Test");
Console.WriteLine(rec); // MyRecord(Id: 1, Name: Test)
```

---

### 7. `Singleton`

**You write:**

```csharp
[Singleton("Awake")]
public partial class GameManager : MonoBehaviour
{
}
```

**Generated code:**

```csharp
public partial class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }
}
```

**How to use:**

```csharp
// Anywhere in your code:
GameManager.Instance.DoSomething();
```

---
