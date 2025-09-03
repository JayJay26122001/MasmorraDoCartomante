using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Windows;

public class CardModularDescription : MonoBehaviour
{
    [SerializeField] TextMeshPro descriptionText;
    CardDisplay cardDisplay;
    Card cardData;
    int cardDamage, currentDamage, currentDefense;
    public struct Token 
    {
        public int index;
        public string var;
    }

    void Start()
    {
        cardDisplay = GetComponentInParent<CardDisplay>();
        cardData = cardDisplay.cardData;
    }

    void Update()
    {
        UpdateCardBoolDescription();
        UpdateValues();
    }

    public void UpdateCardBoolDescription()
    {
        string desc = cardData.Description;
        if(cardData.instantaneous && cardData.limited)
        {
            desc += " INSTANTANEOUS & LIMITED";
        }
        else if(cardData.instantaneous && !cardData.limited)
        {
            desc += " INSTANTANEOUS";
        }
        else if (!cardData.instantaneous && cardData.limited)
        {
            desc += " LIMITED";
        }
        descriptionText.text = desc;
    }

    public void UpdateValues()
    {
        string desc = cardData.Description;
        List<Token> tokens = FindValuesInString(desc);
        foreach(Token token in tokens)
        {
            string value = GetEffectValue(token.index, token.var);
            string pattern = $"v[{token.index}]{{{token.var}}}";
            desc = desc.Replace(pattern, value);
        }
        descriptionText.text = desc;
    }

    public List<Token> FindValuesInString(string input)
    {
        List<Token> tokens = new List<Token>();
        for(int i = 0; i < input.Length; i++)
        {
            if (input[i] == 'v' && i + 1 < input.Length && input[i + 1] == '[')
            {
                i += 2; // skip "v["

                // --- Parse index ---
                int start = i;
                for(; i < input.Length && input[i] != ']'; i++) { }
                int effectIndex = int.Parse(input.Substring(start, i - start));

                // Expect ']'
                if (i < input.Length && input[i] == ']') i++;

                // Expect '{'
                if (i < input.Length && input[i] == '{') i++;

                // --- Parse variable ---
                start = i;
                for (; i < input.Length && input[i] != '}'; i++) { }
                string variable = input.Substring(start, i - start);

                // Expect '}'
                if (i < input.Length && input[i] == '}') i++;

                // Add to results
                tokens.Add(new Token { index = effectIndex, var = variable });
            }
        }
        return tokens;
    } 

    string GetEffectValue(int effectIndex, string variable)
    {
        if(effectIndex < cardData.Effects.Count && effectIndex >= 0) 
        {
            Effect observedEffect = cardData.Effects[effectIndex];
            switch (variable)
            {
                case "Damage":
                    if (observedEffect is DealDamage damage)
                    {
                        //return damage.GetDamage().ToString();
                        return $"<color=#FF5555>{damage.GetDamage()}</color>";
                    }
                    break;
                case "Shield":
                    if (observedEffect is GainShield shield)
                    {
                        //return shield.GetShield().ToString();
                        return $"<color=#55AAFF>{shield.GetShield()}</color>";
                    }
                    break;
            }
        }
        return "Invalid Value";
    }
}
