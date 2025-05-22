# How to use
1. Inport `UnityAttributes.dll` to your unity project
2. Disable all ckeckboxes in `UnityAttributes.dll` import settings panel. Press `Apply` <img width="531" alt="image" src="https://github.com/OctopBP/unity-attributes/assets/17803104/d36caba4-abbe-4adb-8edc-1c9d654c2be4">
3. Add `RoslynAnalyzer` tag to `UnityAttributes.dll` <img width="537" alt="image" src="https://github.com/OctopBP/unity-attributes/assets/17803104/4bd16b92-b8f0-4bc8-9745-8963600ab2cf">

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
