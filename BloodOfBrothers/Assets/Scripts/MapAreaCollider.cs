using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAreaCollider : MonoBehaviour
{
    // Start is called before the first frame update
    private List<PlayerMapAreas> playerMapAreasList = new List<PlayerMapAreas>();
    private List<PlayerMapAreas> enemyMapAreasList = new List<PlayerMapAreas>();
    private void OnTriggerEnter(Collider collider) {
      if (collider.TryGetComponent<PlayerMapAreas>(out PlayerMapAreas playerMapAreas)){
            if (collider.tag == "Infantry")
            {
                playerMapAreasList.Add(playerMapAreas);
            }
            if (collider.tag == "Enemy Infantry")
            {
                enemyMapAreasList.Add(playerMapAreas);
            }
      }
    }
    private void OnTriggerExit(Collider collider)
    {
        if (collider.TryGetComponent<PlayerMapAreas>(out PlayerMapAreas playerMapAreas))
        {
            if (collider.tag == "Infantry")
            {
                playerMapAreasList.Remove(playerMapAreas);
            }
            if (collider.tag == "Enemy Infantry")
            {
                enemyMapAreasList.Remove(playerMapAreas);
            }
        }
    }

    public List<PlayerMapAreas> GetPlayerMapAreasList()
    {
        return playerMapAreasList;
    }

    public List<PlayerMapAreas> GetEnemyMapAreasList() 
    {
        return enemyMapAreasList;
    } 
}
