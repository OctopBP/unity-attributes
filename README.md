# How to use
1. Inport `UnityAttributes.dll` to your unity project
2. Disable all ckeckboxes in `UnityAttributes.dll` import settings panel. Press `Apply` <img width="531" alt="image" src="https://github.com/user-attachments/assets/a95b96d5-2c1e-40c7-94a4-82e5984b696d">
3. Add `RoslynAnalyzer` tag to `UnityAttributes.dll` <img width="537" alt="image" src="https://github.com/user-attachments/assets/241a5db9-26c1-4f5d-ba10-958c5cdee314">

---

## Table of Contents

* [GenConstructor](#genconstructor)
* [MonoReadonly](#monoreadonly)
* [PublicAccessor](#publicaccessor)
* [Readonly](#readonly)
* [Record](#record)
* [ShaderProperty](#shaderproperty)
* [Singleton](#singleton)

---

## GenConstructor

### Description

Automatically generates constructors for a class, including all non-static fields except those marked with `GenConstructorIgnore` .

### How to use

Apply `[GenConstructor]` to your partial class. All non-static fields (except those marked with `[GenConstructorIgnore]` ) will be included as parameters in the generated constructor.

```csharp
[GenConstructor]
public partial class MyClass
{
    private int _value1, _value2;
    public int value3;
    [GenConstructorIgnore] private int _ignored;
    private readonly int _readonlyValue;
}

// Usage
var obj = new MyClass(1, 2, 3, 4);
```

<details>
<summary>Generated code</summary>

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

</details>

---

## MonoReadonly

### Description

Restricts assignment to the field only within Unity MonoBehaviour lifecycle methods: `Awake` , `OnEnable` , `Start` , or `Reset` . Assigning elsewhere will trigger a Roslyn analyzer error.

### How to use

Apply `[MonoReadonly]` to a field in a MonoBehaviour. Only assign to this field in `Awake` , `OnEnable` , `Start` , or `Reset` methods.

```csharp
public class MyComponent : MonoBehaviour
{
    [MonoReadonly]
    private int _score;

    void Awake() { _score = 10; } // Allowed
    void Update() { _score = 20; } // Error: not allowed
}
```

---

## PublicAccessor

### Description

Generates a public property accessor for a private or protected field.

### How to use

Apply `[PublicAccessor]` to a private or protected field in a partial class. A public property will be generated for that field.

```csharp
public partial class MyClass
{
    [PublicAccessor] private int _value;
}

// Usage
var obj = new MyClass();
int v = obj.Value; // Accesses the private _value field
```

<details>
<summary>Generated code</summary>

```csharp
public int Value => _value;
```

</details>

---

## Readonly

### Description

Prevents assignment to the field outside of its declaration (enforced by analyzer). Useful for enforcing immutability in Unity projects.

### How to use

Apply `[Readonly]` to a field. You can only assign to this field at its declaration.

```csharp
public class MyClass
{
    [Readonly]
    private int _score = 10;
}

// Usage
// _score = 20; // Error: assignment not allowed
```

---

## Record

### Description

Generates a record-like class with value equality, `ToString` , and other utility methods, similar to C# 9 `record` but for older C# or Unity compatibility.

### How to use

Apply `[Record]` to your partial class. The generator will create constructors, `Equals` , `GetHashCode` , and `ToString` methods.

```csharp
[Record]
public partial class MyRecord
{
    public int Id;
    public string Name;
}

// Usage
var rec = new MyRecord(1, "Test");
Console.WriteLine(rec); // MyRecord(Id: 1, Name: Test)
```

<details>
<summary>Generated code</summary>

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

</details>

---

## ShaderProperty

### Description

Generates static shader property IDs and helper methods for setting/getting shader properties on Unity materials, global shader properties, MaterialPropertyBlocks, or ComputeShaders. Supports multiple property types including Float, Integer, Bool, Color, Vector, Matrix, Texture, and arrays.

### How to use

Apply `[ShaderProperty]` attributes to your partial class. Each attribute defines a shader property with a name, type, and optional mode. You can apply multiple `[ShaderProperty]` attributes to the same class.

**Parameters:**
- `name` (string): The shader property name as it appears in the shader
- `type` (ShaderPropertyType): The property type (Float, Integer, Bool, Color, Vector, Matrix, Texture, Buffer, ConstantBuffer, FloatArray, ColorArray, VectorArray, MatrixArray)
- `mode` (ShaderPropertyMode, optional): Access mode - Default (Material), Global (Shader global), WithPropertyBlock (MaterialPropertyBlock extension), or Compute (ComputeShader)

```csharp
using UnityAttributes.ShaderProperty;

[ShaderProperty("_MainColor", ShaderPropertyType.Color)]
[ShaderProperty("_Metallic", ShaderPropertyType.Float)]
[ShaderProperty("_GlowIntensity", ShaderPropertyType.Float, ShaderPropertyMode.Global)]
public partial class MyShaderProperties
{
}

// Usage - Default mode (Material)
var material = GetComponent<Renderer>().material;
MyShaderProperties.SetMainColor(material, Color.red);
Color color = MyShaderProperties.GetMainColor(material);

// Usage - Global mode
MyShaderProperties.SetGlowIntensity(1.5f);
float intensity = MyShaderProperties.GetGlowIntensity();

// Usage - WithPropertyBlock mode
var propertyBlock = new MaterialPropertyBlock();
propertyBlock.SetMetallic(0.8f);
float metallic = propertyBlock.GetMetallic();
```

<details>
<summary>Generated code</summary>

```csharp
public partial class MyShaderProperties
{
    public static readonly int MainColor = Shader.PropertyToID("_MainColor");
    
    public static void SetMainColor(Material material, Color value)
    {
        material.SetColor(MainColor, value);
    }
    
    public static Color GetMainColor(Material material)
    {
        return material.GetColor(MainColor);
    }
    
    public static readonly int Metallic = Shader.PropertyToID("_Metallic");
    
    public static void SetMetallic(Material material, float value)
    {
        material.SetFloat(Metallic, value);
    }
    
    public static float GetMetallic(Material material)
    {
        return material.GetFloat(Metallic);
    }
    
    public static readonly int GlowIntensity = Shader.PropertyToID("_GlowIntensity");
    
    public static void SetGlowIntensity(float value)
    {
        Shader.SetGlobalFloat(GlowIntensity, value);
    }
    
    public static float GetGlowIntensity()
    {
        return Shader.GetGlobalFloat(GlowIntensity);
    }
}
```

</details>

---

## Singleton

### Description

Implements the Unity MonoBehaviour singleton pattern. The static `Instance` is set in the specified initialization method (e.g., `Awake` ).

### How to use

Apply `[Singleton("Awake")]` to your partial MonoBehaviour class. The generator will create a static `Instance` property and set it in the specified method.

```csharp
[Singleton("Awake")]
public partial class GameManager : MonoBehaviour
{
}

// Usage
GameManager.Instance.DoSomething();
```

<details>
<summary>Generated code</summary>

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

</details>

---
