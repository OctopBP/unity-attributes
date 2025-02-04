﻿//HintName: ColorTestForClassWithT_List_Int.g.cs
/// <auto-generated />

[System.Serializable]
public class ColorTestForClassWithT_List_Int
{
    [UnityEngine.SerializeField] private ClassWithT<List<Int>> Red;
    [UnityEngine.SerializeField] private ClassWithT<List<Int>> Blue;
    [UnityEngine.SerializeField] private ClassWithT<List<Int>> Green;

    public ColorTestForClassWithT_List_Int() { }

    public ColorTestForClassWithT_List_Int(ClassWithT<List<Int>> red, ClassWithT<List<Int>> blue, ClassWithT<List<Int>> green)
    {
        this.Red = red;
        this.Blue = blue;
        this.Green = green;
    }

    public ClassWithT<List<Int>> Get(ColorTest key)
    {
        return key switch
        {
            ColorTest.Red => Red,
            ColorTest.Blue => Blue,
            ColorTest.Green => Green,
            _ => throw new System.ArgumentOutOfRangeException(nameof(key), key, null),
        };
    }

    public void Set(ColorTest key, ClassWithT<List<Int>> value)
    {
        switch (key)
        {
            case ColorTest.Red: Red = value; break;
            case ColorTest.Blue: Blue = value; break;
            case ColorTest.Green: Green = value; break;
            default: throw new System.ArgumentOutOfRangeException(nameof(key), key, null);
        }
    }

    public void Apply(ColorTest key, System.Func<ClassWithT<List<Int>>, ClassWithT<List<Int>>> func)
    {
        switch (key)
        {
            case ColorTest.Red: Red = func(Red); break;
            case ColorTest.Blue: Blue = func(Blue); break;
            case ColorTest.Green: Green = func(Green); break;
            default: throw new System.ArgumentOutOfRangeException(nameof(key), key, null);
        }
    }

    public ClassWithT<List<Int>>[] Values => new[]
    {
        Red,
        Blue,
        Green,
    };
}
