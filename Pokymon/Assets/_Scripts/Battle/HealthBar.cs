using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]private GameObject healthBar;

    public Color barColor(float finalScale)
    {
        if (finalScale < 0.15f)
        {
                return Color.red;
        }
        else if (finalScale < 0.5f) 
        {
                return Color.yellow; 
        }
        else
        {
                return Color.green;
        }
    }

    /// <summary>
    /// Actualiza la barra de vida a partir del valor normalizado de la misma
    /// </summary>
    /// <param name="normalizedValue">Valor de la vida noralizado entre 0 y 1</param>
    public void SetHP(float normalizedValue)
    {
        healthBar.transform.localScale = new Vector3(normalizedValue, 1.0f);
        healthBar.GetComponent<Image>().color = barColor(normalizedValue);
    }

    public IEnumerator SetSmoothHp(float normalizedValue)
    {
        /*float currentScale = healthBar.transform.localScale.x;
        float updateQuantity = currentScale - normalizedValue;
        while (currentScale - normalizedValue > Mathf.Epsilon)
        {
            currentScale -= updateQuantity * Time.deltaTime;
            healthBar.transform.localScale = new Vector3(currentScale, 1);
            healthBar.GetComponent<Image>().color = barColor;
            yield return null;
        }
        healthBar.transform.localScale = new Vector3(normalizedValue, 1);
        */

        var seq = DOTween.Sequence();
        seq.Append(healthBar.transform.DOScaleX(normalizedValue, 1f)); 
        seq.Join(healthBar.GetComponent<Image>().DOColor(barColor(normalizedValue), 1f));
        yield return seq.WaitForCompletion();
    }
}
