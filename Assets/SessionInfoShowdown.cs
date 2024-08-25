using System.Collections;
using System.Collections.Generic;
using GWBase;
using TMPro;
using UnityEngine;

public class SessionInfoShowdown : MonoBehaviour
{
    TextMeshProUGUI textField = null;
    // Start is called before the first frame update
    void Start()
    {
        textField = GetComponent<TextMeshProUGUI>();
        var sessionInformation = GameManager.sessionInformation;
        textField.text = $"Scores: \n  Kill Count: {sessionInformation.killCount}, Total Damage: {sessionInformation.totalDamageGiven}, Total Hits: {sessionInformation.totalHitsGiven} \n Total Damage Taken: {sessionInformation.totalDamageTaken}, Total Hits Taken: {sessionInformation.totalHitsTaken}, Total XP: {sessionInformation.totalXP}";
        StartCoroutine(StatUpdater());
    }
    
    public IEnumerator StatUpdater() {
        while (true) {
        var sessionInformation = GameManager.sessionInformation;
        textField.text = $"Scores: \n  Kill Count: {sessionInformation.killCount}, Total Damage: {sessionInformation.totalDamageGiven}, Total Hits: {sessionInformation.totalHitsGiven} \n Total Damage Taken: {sessionInformation.totalDamageTaken}, Total Hits Taken: {sessionInformation.totalHitsTaken}, Total XP: {sessionInformation.totalXP}";
        yield return new WaitForSeconds(0.5f);
        }
    }
}
