using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour {

    private Enemigo padre;

    private void Start() 
    {
        padre = transform.parent.transform.parent.GetComponent<Enemigo>();
    }
    //Asigna las pildoras que se encuentran dentro del sensor, al array de pildoras del enemigo 
    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.tag == "Bonus" && !padre.targets.Contains(collision.transform)) {
            padre.targets.Add(collision.transform);        
        }
    }
    //Elimina las pildoras que salen del sensor del enmigo, de la lista de pildoras
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.tag == "Bonus") {
            padre.targets.Remove(collision.transform);

        }
    }
}
