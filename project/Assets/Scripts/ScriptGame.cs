using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ScriptGame : MonoBehaviour
{
    /// <summary>
    /// Cообщение, которое отображает текущую руку и текущий счет денег игроков.
    /// </summary>
    public TMP_Text[] playersText;
    /// <summary>
    /// Отображение имен игроков (по умолчанию игрок 1 и так далее).
    /// </summary>
    public TMP_Text[] playersNameText;
    /// <summary>
    /// Кнопка для добора карты.
    /// </summary>
    public Button HitButton;
    /// <summary>
    /// Кнопка для раздачи карт.
    /// </summary>
    public Button DealButton;
    /// <summary>
    /// Кнопка для пропуска хода.
    /// </summary>
    public Button standButton;
    /// <summary>
    /// Кнопка с определенной ставкой.
    /// </summary>
    public Button betFirstButton;
    /// <summary>
    /// Кнопка с определенной ставкой.
    /// </summary>
    public Button betSecondButton;
    /// <summary>
    /// Кнопка с определенной ставкой.
    /// </summary>
    public Button betThirdButton;
    /// <summary>
    /// Кнопка для подсчета очков игроков и оплаты выигрыша.
    /// </summary>
    public Button CalculateButton;
    /// <summary>
    /// Кнопка для удвоения ставки игрока.
    /// </summary>
    public Button doubleButton;
    /// <summary>
    /// Кнопка для начала следующего раунда.
    /// </summary>
    public Button nextRoundButton;
    /// <summary>
    /// Кнопка для сдачи карт игрока.
    /// </summary>
    public Button surrenderButton;
    /// <summary>
    /// Объект класса Deck. В нем хранятся массивы картинок каждой карты и значения каждой карты.
    /// </summary>
    public ScriptDeck deck;
    /// <summary>
    /// Массив картинок карт игроков. В данный массив значений происходит присвоение конкретной рандомной карты.
    /// </summary>
    public Image[] playerCardImages;
    /// <summary>
    /// Массив игроков. Данный класс содержит много методов, которые используются для правильных ходов в игре.
    /// Можно посмотреть описание каждого метода в классе PlayerScript.
    /// </summary>
    private ScriptPlayer[] players;
    /// <summary>
    /// Словарь, в коротом первый элемент - индекс игрока, второй элемент - список индексов карт которые уже разданы.
    /// При доборе карт происходит проверка текущего индекса игрока и индекса последней разданной карты и новая карта становится на lastindex + 1. 
    /// </summary>
    private Dictionary<int, List<int>> ValuesCard;
    /// <summary>
    /// Изображение скрытой карты дилера.
    /// </summary>
    private Sprite hideCard;
    public Image imageHideCard;
    /// <summary>
    /// Значение скрытой карты дилера.
    /// </summary>
    private int openCard;
    /// <summary>
    /// Индекс дилера. То есть это последний индекс в массиве PlayerScript. Player.Length - 1.
    /// </summary>
    public int DealerIndex;
    /// <summary>
    /// Индекс текущего игрока
    /// </summary>
    public int PlayerIndex;
    
    /// <summary>
    /// <b>В методе Start происходят начальные действия для начала игры.</b><br></br>
    /// (получаем количество игроков, передаем в массив players, кнопки деактивируем, присваиваем имена игрокам)
    /// </summary>
    public void Start()
    {
        int selectedOption = 1;
        selectedOption += 1;
        players = new ScriptPlayer[selectedOption];
        ValuesCard = new Dictionary<int, List<int>>();
        DealerIndex = players.Length - 1;

        for (int i = 0; i < players.Length; i++)
        {
            playersText[i].text = "";
            players[i] = new ScriptPlayer();
            playersNameText[i].text = i == DealerIndex ? "Дилер" : $"Игрок {i + 1}";
        }

        foreach (Image image in playerCardImages)
        {
            image.gameObject.SetActive(false);
        }

        VisibleButton(HitButton, false);
        VisibleButton(standButton, false);
        VisibleButton(DealButton, false);
        VisibleButton(CalculateButton, false);
        VisibleButton(nextRoundButton, false);
        VisibleButton(doubleButton, false);
        VisibleButton(surrenderButton, false);

        DealButton.onClick.AddListener(DealCards);
        HitButton.onClick.AddListener(HitCards);
        standButton.onClick.AddListener(StandCards);
        betFirstButton.onClick.AddListener(() => Bet(20));
        betSecondButton.onClick.AddListener(() => Bet(50));
        betThirdButton.onClick.AddListener(() => Bet(100));
        CalculateButton.onClick.AddListener(CalculatePoints);
        nextRoundButton.onClick.AddListener(NextRound);
        doubleButton.onClick.AddListener(DoubleBet);
        surrenderButton.onClick.AddListener(Surrender);

        playersText[0].text = $"Денег: {players[PlayerIndex].GetMoney()}";
    }

    /// <summary>
    /// <b>В методе Update меняем цвет имени текущего игрока для того, чтобы понимать кто текущий игрок и кто должен делать ход.</b>
    /// </summary>
    public void Update()
    {
        for (int i = 0; i < playersNameText.Length; i++)
        {
            playersNameText[i].color = i == PlayerIndex ? Color.white : Color.gray;
        }
        if(PlayerIndex == DealerIndex)
        {
            VisibleButton(doubleButton, false);
            VisibleButton(surrenderButton, false);
            VisibleButton(standButton, players[DealerIndex].GetHand() >= 17 && players[DealerIndex].GetHand() <= 21);
        }
    }

    /// <summary>
    /// <b>При вызове метода Surrender игрок сдается от дальнейшей игры. Возвращается 1/2 его ставки.</b>
    /// </summary>
    private void Surrender()
    {
        players[PlayerIndex].SetCheckPay(true);
        int pay = players[PlayerIndex].GetBetAmount() / 2;
        players[PlayerIndex].SetMoney(pay);
        playersText[PlayerIndex].text = $"Игрок сдался. Денег: {players[PlayerIndex].GetMoney()}";
        NextRound();
    }

    /// <summary>
    /// <b>В методе DoubleBet происходит удвоение ставки игрока.</b>
    /// </summary>
    private void DoubleBet()
    {
        int bet = players[PlayerIndex].GetBetAmount();
        players[PlayerIndex].BetMoney(bet);
        int index = PlayerIndex;
        HitCards();
        if (index == PlayerIndex) PlayerIndex += 1;
    }

    /// <summary>
    /// <b>В методе StandCards происходит пропуск хода игрока.</b>
    /// </summary>
    private void StandCards()
    {
        PlayerIndex += 1;
        VisibleButton(surrenderButton, false);
        VisibleButton(doubleButton, false);
        if (PlayerIndex == players.Length)
        {
            VisibleButton(CalculateButton, true);
            VisibleButton(standButton, false);
            VisibleButton(HitButton, false);
        }

        else if (PlayerIndex == DealerIndex)
        {
            playerCardImages[PlayerIndex * 6].sprite = hideCard;
            PrintCurrentText(PlayerIndex);
        }
    }

    /// <summary>
    /// <b>В методе NextRound происходит начало нового раунда.</b>
    /// </summary>
    private void NextRound()
    {
        PlayerIndex = 0;
        foreach (var entry in ValuesCard)
        {
            foreach (int cardIndex in entry.Value)
            {
                playerCardImages[cardIndex].gameObject.SetActive(false);
            }
            players[entry.Key].SetHand(0);
        }
        ValuesCard.Clear();
        foreach (var t in players)
        {
            t.SetCheckPay(false);
        }

        for (int i = 0; i < players.Length; i++)
        {
            playersText[i].ClearMesh();
        }

        hideCard = null;
        playerCardImages[DealerIndex * 6].sprite = imageHideCard.sprite;

        VisibleButton(betFirstButton, true);
        VisibleButton(betSecondButton, true);
        VisibleButton(betThirdButton, true);
        VisibleButton(CalculateButton, false);
        VisibleButton(nextRoundButton, false);
        VisibleButton(standButton, false);
        VisibleButton(doubleButton, false);
        VisibleButton(surrenderButton, false);
        VisibleButton(HitButton, false);
        
        playersText[0].text = $"Денег: {players[PlayerIndex].GetMoney()}";
    }

    /// <summary>
    /// <b>В методе DealCards происходит раздача двух карт каждому игроку.</b>
    /// </summary>
    private void DealCards()
    {
        PlayerIndex = 0;
        for (int i = 0; i < players.Length; i++)
        {
            List<int> cardIndices = new List<int>();
            for (int j = 0; j < 2; j++)
            {
                int randomIndex = deck.GetUniqueCard();
                if (i == DealerIndex && j == 0)
                {
                    playerCardImages[i * 6 + j].gameObject.SetActive(true);
                    hideCard = deck.Cards[randomIndex];
                }
                else
                {
                    playerCardImages[i * 6 + j].gameObject.SetActive(true);
                    playerCardImages[i * 6 + j].sprite = deck.Cards[randomIndex];
                    openCard = deck.ValuesOfCards[randomIndex];
                }

                players[i].AceCheck(deck.ValuesOfCards[randomIndex]);
                cardIndices.Add(i * 6 + j);
            }
            ValuesCard[i] = cardIndices;
            PrintCurrentText(i);
        }

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetHand() != 21)
            {
                PlayerIndex = i;
                break;
            }
        }

        for (int i = 0; i < players.Length; i++)
        {
            CalculatePointsBlackJack(i);
        }

        if (openCard != 11) VisibleButton(surrenderButton, true);

        VisibleButton(DealButton, false);
        VisibleButton(HitButton, true);
        VisibleButton(standButton, true);
        VisibleButton(surrenderButton, true);

        if (PlayerIndex != DealerIndex)
        {
            VisibleButton(doubleButton, true);
        }
    }

    /// <summary>
    /// <b>В методе PrintCurrentText происходит вывод текущей информаиции об игроке.</b>
    /// </summary>
    /// <param name="index">Текущий игрок</param>
    /// <returns>Сообщение о текущем состоянии игрока</returns>
    private void PrintCurrentText(int index)
    {
        if (index != DealerIndex)
        {
            playersText[index].text = $"Игрок: {players[index].GetHand()}\nДенег {players[index].GetMoney()}\n";
        }
        else
        {
            playersText[index].text = playerCardImages[index * 6].sprite == hideCard ? $"Дилер: {players[index].GetHand()}\n" : "";
        }
    }

    /// <summary>
    /// <b>В методе VisibleButton происходит активация и деактивация кнопок.</b>
    /// </summary>
    /// <param name="button">Название кнопки</param>
    /// <param name="flag">Состояние кнопки. true - активная, false - неактивная.</param>
    private void VisibleButton(Button button, bool flag)
    {
        button.gameObject.SetActive(flag);
    }

    /// <summary>
    /// <b>В методе CalculatePointsBlackJack происходит проверка на блекджек и выплата выигрыша игроку в случае блекджека</b>
    /// </summary>
    /// <param name="index">Индекс текущего игрока</param>
    private void CalculatePointsBlackJack(int index)
    {
        if (index != DealerIndex)
        {
            if (players[index].GetHand() == 21 && openCard < 10)
            {
                players[index].SetCheckPay(true);
                int pay = players[index].GetBetAmount() * 3 / 2 + players[index].GetBetAmount();
                players[index].SetMoney(pay);
                PrintCurrentText(index);
                NextRound();
            }
        }
    }

    /// <summary>
    /// <b>В методе CalculatePoints происходит подсчет очков всех игроков и определение победителей.</b>
    /// </summary>
    private void CalculatePoints()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (i != DealerIndex)
            {
                if (!players[i].GetCheckPay())
                {
                    if (players[i].GetHand() > 21 || (players[i].GetHand() < players[DealerIndex].GetHand() && players[DealerIndex].GetHand() < 22))
                    {
                        players[i].SetCheckPay(true);
                        playersText[i].text = $"Игрок проиграл. Рука: {players[i].GetHand()}. Денег: {players[i].GetMoney()}. ";
                    }
                    else if ((players[i].GetHand() > players[DealerIndex].GetHand() && players[DealerIndex].GetHand() < 22) || (players[DealerIndex].GetHand() > 21 && players[i].GetHand() < 22))
                    {
                        int pay = players[i].GetBetAmount() + players[i].GetBetAmount();
                        players[i].SetMoney(pay);
                        players[i].SetCheckPay(true);
                        playersText[i].text = $"Игрок выиграл. Рука: {players[i].GetHand()}. Денег: {players[i].GetMoney()}. ";
                    }
                    else if (players[i].GetHand() == players[DealerIndex].GetHand())
                    {
                        int pay = players[i].GetBetAmount();
                        players[i].SetMoney(pay);
                        players[i].SetCheckPay(true);
                        playersText[i].text = $"Игрок сыграл в ничью. Рука: {players[i].GetHand()}. Денег: {players[i].GetMoney()}. ";
                    }
                }
            }
            //else if(i == dilerIndex) playersText[i].text = $"Дилер. Рука: {players[i].GetHand()}. ";
        }
        VisibleButton(nextRoundButton, true);
        VisibleButton(CalculateButton, false);
        VisibleButton(doubleButton, false);
    }

    /// <summary>
    /// <b>В методе HitCards происходит добор карты текущего игрока.</b>
    /// </summary>
    private void HitCards()
    {
        VisibleButton(doubleButton, false);
        int randomIndex = deck.GetUniqueCard();
        int lastCardIndex = ValuesCard[PlayerIndex][ValuesCard[PlayerIndex].Count - 1];
        ValuesCard[PlayerIndex].Add(lastCardIndex + 1);
        playerCardImages[lastCardIndex + 1].gameObject.SetActive(true);
        playerCardImages[lastCardIndex + 1].sprite = deck.Cards[randomIndex];
        players[PlayerIndex].AceCheck(deck.ValuesOfCards[randomIndex]);
        PrintCurrentText(PlayerIndex);
        if (players[PlayerIndex].GetHand() >= 21)
        {
            if (PlayerIndex != DealerIndex)
            {
                PlayerIndex += 1;
                VisibleButton(surrenderButton, false);
                VisibleButton(doubleButton, true);
                VisibleButton(doubleButton, true);
                if (PlayerIndex == DealerIndex)
                {
                    playerCardImages[PlayerIndex * 6].sprite = hideCard;
                    PrintCurrentText(PlayerIndex);
                }
            }

            else if (PlayerIndex == DealerIndex)
            {
                VisibleButton(CalculateButton, true);
                VisibleButton(HitButton, false);
                VisibleButton(standButton, false);
                VisibleButton(surrenderButton, false);
            }
        }
    }

    /// <summary>
    /// <b>В методе Bet происходит ставка игрока.</b>
    /// </summary>
    /// <param name="bet">Ставка</param>
    private void Bet(int bet)
    {
        players[PlayerIndex].BetMoney(bet);
        PrintCurrentText(PlayerIndex);
        PlayerIndex += 1;
        if (PlayerIndex == DealerIndex)
        {
            VisibleButton(DealButton, true);
            VisibleButton(betFirstButton, false);
            VisibleButton(betSecondButton, false);
            VisibleButton(betThirdButton, false);
        }
    }
}