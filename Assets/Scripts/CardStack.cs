using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardStack<CardData> : List<CardData>
{
    public CardStack() : base() {}
    public CardStack(IEnumerable<CardData> cards) : base(cards) {}
}

public static class CardStackExtensions
{
    public static void Push(this CardStack<CardData> stack, CardData cardData)
    {
        stack.Add(cardData);
    }

    public static CardData Pop(this CardStack<CardData> stack)
    {
        if(stack.Count == 0) { return null; }
        var result = stack[stack.Count - 1];
        stack.RemoveAt(stack.Count - 1);
        return result;
    }

    public static CardData Peek(this CardStack<CardData> stack)
    {
        if(stack.Count == 0) { return null; }
        return stack[stack.Count - 1];
    }

    public static CardData DrawAtRandom(this CardStack<CardData> stack)
    {
        if(stack.Count == 0) { return null; }
        var rand = UnityEngine.Random.Range(0, stack.Count);

        var result = stack[rand];
        stack.RemoveAt(rand);
        return result;
    }
}
