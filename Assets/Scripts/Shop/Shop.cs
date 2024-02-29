using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Player;
using UnityEngine.Scripting;
using System.IO;
using System;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class Item
{
    public string name;
    public int price;

    public Item()
    {
        name = "Item";
        price = 0;
    }

    public Item(string name, int price)
    {
        this.name = name;
        this.price = price;
    }
}

public class Shop : MonoBehaviour
{
    private static string ITEMS_PURCHASED_FILE = "shop-purchased";

    private static string PURCHASED_REFUSED_IMAGE_PATH = "BuyButton/X";

    private static List<Item> itemsForSale = new List<Item>{
        new Item("AK-47",1000),
        new Item("Revolver",1500),
        new Item("Shotgun",2000),
        new Item("SubMachineGun",1500),
    };

    [SerializeField] private PlayerController player;

    [SerializeField] private GameObject itemShowCasePrefab;

    private List<Item> itemsPurchased;

    //Items that are on sale paired with their respective showcase gameobject
    private List<(Item,GameObject)> itemSelectedForSale;

    private RectTransform itemHolder;

    private TextMeshProUGUI moneyText;

    //---------------------------------unity events
    private void Awake()
    {
        itemHolder = transform.Find("Panel/ItemHolder").GetComponent<RectTransform>();

        moneyText = transform.Find("Panel/Money").GetComponent<TextMeshProUGUI>();

        if (itemsForSale == null)
        {
            itemsForSale = new List<Item>();
        }

        try
        {
            itemsPurchased = GameSaver.LoadData<List<Item>>(ITEMS_PURCHASED_FILE);
        }
        catch (FileNotFoundException)
        {
            itemsPurchased = new List<Item>();
            GameSaver.SaveData(ITEMS_PURCHASED_FILE, itemsPurchased);
        }

        SelectItemsForSale();
        UpdatePlayerShowcaseMoney();
    }

    //----------------------------------general functions

    private void SelectItemsForSale()
    {
        List<Item> tempItemsForSale = new();

        //All items that haven't been purchased
        foreach (Item item in itemsForSale)
        {
            if (item != null && !itemsPurchased.Contains(item))
            {
                tempItemsForSale.Add(item);
            }
        }

        //Take only 3 items from the list that have not been purchased
        while (tempItemsForSale.Count >= 4)
        {
            tempItemsForSale.RemoveAt(UnityEngine.Random.Range(0, tempItemsForSale.Count - 1));
        }

        itemSelectedForSale = new();

        //get item showcase for each item
        foreach (Item item in tempItemsForSale)
        {
            itemSelectedForSale.Add((item, ShowcaseItem(item)));
        }

        GameObject ShowcaseItem(Item item)
        {
            GameObject itemShowCase = Instantiate(itemShowCasePrefab, itemHolder);
            itemShowCase.SetActive(true);
            itemShowCase.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.name;
            itemShowCase.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = item.price.ToString();
            itemShowCase.transform.Find("BuyButton").GetComponent<Button>().onClick.AddListener(() => BuyItem(tempItemsForSale.IndexOf(item)));

            return itemShowCase;
        }
    }

    public void BuyItem(int index)
    {
        if (player.GetMoney() >= itemSelectedForSale[index].Item1.price)
        {
            player.AddMoney(-itemSelectedForSale[index].Item1.price);
            player.PickUpWeapon(itemSelectedForSale[index].Item1.name);

            itemsPurchased.Add(itemSelectedForSale[index].Item1);
            GameSaver.SaveData(ITEMS_PURCHASED_FILE, itemsPurchased);

            UpdatePlayerShowcaseMoney();
            Debug.Log("Item purchased");
        }
        else
        {
            ShowTooPoor(itemSelectedForSale[index].Item2);
            Debug.Log("Not enough money");
        }
    }

    private async void ShowTooPoor(GameObject itemShowCase)
    {
        itemShowCase.transform.Find(PURCHASED_REFUSED_IMAGE_PATH).gameObject.SetActive(true);
        await Task.Delay(1000);
        itemShowCase.transform.Find(PURCHASED_REFUSED_IMAGE_PATH).gameObject.SetActive(false);
    }

    public void UpdatePlayerShowcaseMoney()
    {
        moneyText.text = "Money: " + player.GetMoney().ToString();
    }

    //---------------------------------setters

    public void ClearItemsPurchased()
    {
        itemsPurchased.Clear();
        GameSaver.SaveData(ITEMS_PURCHASED_FILE, itemsPurchased);
    }

    //---------------------------------getters
    public List<Item> GetItemsForSale()
    {

        if (itemsForSale == null){
            Debug.Log("itemsForSale is null");
            itemsForSale = new List<Item>();}
            
        return itemsForSale;
    }

    public List<Item> GetItemsPurchased()
    {
        return itemsPurchased;
    }
}
