using System.Collections.Generic;
using UnityEngine;

public class ScriptDeck : MonoBehaviour
{
    [SerializeField]
    private Sprite[] cards;
    /// <summary>
    /// Массив Cards содержит картинки карт.
    /// </summary>
    public Sprite[] Cards => cards;

    [SerializeField]
    private int[] valuesOfCards;
    /// <summary>
    /// Массив ValueOfCards содержит значения карт.
    /// </summary>
    public int[] ValuesOfCards => valuesOfCards;

    /// <summary>
    /// Список selectedCards содержит значения индексов карт, которые были разданы. 
    /// </summary>
    private List<int> selectedCards = new();

    /// <summary>
    /// Метод GetUniqueCard возвращает уникальную карту.
    /// </summary>
    /// <returns>Индекс уникальной карты</returns>
    public int GetUniqueCard()
    {
        int randomIndex;
        //Выполняем действие, пока индекс в списке, тем самым проходимся по списку и ждем когда этого индекса нет в списке, получается достаем уникальный индекс
        do
        {
            randomIndex = Random.Range(0, Cards.Length);
        } while (selectedCards.Contains(randomIndex));

        selectedCards.Add(randomIndex);
        return randomIndex;
    }
}