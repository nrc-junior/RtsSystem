using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpendSystem : MonoBehaviour {
   public RTSPlayer player {get; set;}
   public Dictionary<ResourceData, PlayerResource> playerBag; 
   
   public bool CanAfford(ResourcePrice[] totalPrice){
      foreach (ResourcePrice price in totalPrice){

         if(!playerBag.ContainsKey(price.resource) || playerBag[price.resource].quantity < price.cost){
            Debug.Log($"não tem {price.cost} de {price.resource.name}");
            return false;
         } 
      }
      return true;
   }

   public void Pay(ResourcePrice[] totalPrice){
      foreach (ResourcePrice price in totalPrice){
         playerBag[price.resource].quantity -= price.cost;
         player.menu.resourceLayout.SetValue(price.resource, playerBag[price.resource].quantity);
      }
   }
    
}
