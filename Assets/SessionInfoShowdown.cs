using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GWBase;
using TMPro;
using UnityEngine;

public class SessionInfoShowdown : MonoBehaviour
{
    TextMeshProUGUI textField = null;
    public bool shouldUpdateStats;
    // Start is called before the first frame update
    void Start()
    {
        textField = GetComponent<TextMeshProUGUI>();
        var sessionInformation = GameManager.gameManager.sessionInformation;
        textField.text = $"Scores: \n  Kill Count: {sessionInformation.killCount}, Total Damage: {sessionInformation.totalDamageGiven}, Total Hits: {sessionInformation.totalHitsGiven} \n Total Damage Taken: {sessionInformation.totalDamageTaken}, Total Hits Taken: {sessionInformation.totalHitsTaken}, Total XP: {sessionInformation.totalXP}";
        UpdateStat();
    }

    public async Task UpdateStat() {
        while (shouldUpdateStats) {
            var sessionInformation = GameManager.gameManager.sessionInformation;
            textField.text = $"Scores: \n  Kill Count: {sessionInformation.killCount}, Total Damage: {sessionInformation.totalDamageGiven}, Total Hits: {sessionInformation.totalHitsGiven} \n Total Damage Taken: {sessionInformation.totalDamageTaken}, Total Hits Taken: {sessionInformation.totalHitsTaken}, Total XP: {sessionInformation.totalXP}";
            await Task.Delay(500);
        }
    }
}
