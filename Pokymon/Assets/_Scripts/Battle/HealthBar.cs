using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]private GameObject healthBar;
    public Text currentHpText;
    public Text maxHpText;

    /// <summary>
    /// Actualiza la barra de vida a partir del valor normalizado de la misma
    /// </summary>
    /// <param name="normalizedValue">Valor de la vida noralizado entre 0 y 1</param>
    public void SetHP(Pokemon pokemon)
    {
        float normalizedValue = (float) pokemon.HP / pokemon.MaxHp;
        healthBar.transform.localScale = new Vector3(normalizedValue, 1.0f);
        healthBar.GetComponent<Image>().color = ColorManager.SharedInstance.barColor(normalizedValue);
        currentHpText.text = pokemon.HP.ToString();
        maxHpText.text = $"/{pokemon.MaxHp}";
    }

    public IEnumerator SetSmoothHp(Pokemon pokemon)
    {
        float normalizedValue = (float) pokemon.HP / pokemon.MaxHp;
        var seq = DOTween.Sequence();
        seq.Append(healthBar.transform.DOScaleX(normalizedValue, 1f)); 
        seq.Join(healthBar.GetComponent<Image>().DOColor(ColorManager.SharedInstance.barColor(normalizedValue), 1f));
        seq.Join(currentHpText.DOCounter(pokemon.previousHpValue, pokemon.HP, 1f));
        yield return seq.WaitForCompletion();
    }
}
