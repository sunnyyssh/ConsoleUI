namespace Sunnyyssh.ConsoleUI.Tests;

public class ElementsFieldBuilderTests
{
    [Fact]
    public void Dense_Places()
    {
        const int count = 10;
        const double deltaWidth = 1.0 / count;
    
        var builder = new ElementsFieldBuilder(count, 1, false);
        var rect = new RectangleBuilder(deltaWidth, 1.0, Color.Default);

        var resultRects = new ChildInfo[count];
        for (int i = 0; i < 10; i++)
        {
            builder.Place(rect, new Position(deltaWidth * i, 0), out resultRects[i]);
        }

        for (int i = 0; i < count; i++)
        {
            Assert.Equal(1, resultRects[i].Width);
            Assert.Equal(i, resultRects[i].Left);
        }
    }
    
    [Fact]
    public void Overlapping_Throws()
    {
        var builder = new ElementsFieldBuilder(10, 10, false);
        
        builder.Place(new RectangleBuilder(Size.FullSize, Color.Default), Position.LeftTop);

        Assert.Throws<ChildPlacementException>(() =>
        {
            builder.Place(new RectangleBuilder(1, 1, Color.Default), Position.LeftTop);
        });
    }
}