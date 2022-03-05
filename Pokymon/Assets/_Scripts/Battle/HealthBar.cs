using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]private GameObject healthBar;

    /// <summary>
    /// Actualiza la barra de vida a partir del valor normalizado de la misma
    /// </summary>
    /// <param name="normalizedValue">Valor de la vida noralizado entre 0 y 1</param>
    public void SetHP(float normalizedValue)
    {
        healthBar.transform.localScale = new Vector3(normalizedValue, 1.0f);
        healthBar.GetComponent<Image>().color = ColorManager.SharedInstance.barColor(normalizedValue);
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
        seq.Join(healthBar.GetComponent<Image>().DOColor(ColorManager.SharedInstance.barColor(normalizedValue), 1f));
        yield return seq.WaitForCompletion();
    }
}
