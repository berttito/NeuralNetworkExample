using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedNeuronalNaves : MonoBehaviour {
    private RedNeuronal red;
    public int numInput = 4, numHidden = 3, numOutput = 3;
    public bool randomLearn;

    // INPUT: Diferencia contador, distancia player, atacando, distancia a la pill
    // OUTPUT: Recolectar, Atacar, Escapar

    
    //Matriz de entrenamiento para la red neuronal
    private static float[,] entrenamiento =
    {
        //diferencia pequeña en las pildoras ,distancia al player max,no ataca, pildora a distancia media=recolecta
            {0.05f, 1, 0, 0.5f,                     0.9f, 0.1f, 0.1f},

       //np diferencia de pildoras,distancia cercana, no ataca, pildora cercana=recolecta
            {0, 0.2f, 0, 0.2f,                      0.9f, 0.1f, 0.1f},

        //diferencia de pildoras 15, distancia media, no ataca, pildora cercana=atacar
            {0.2f, 0.5f, 0, 0.4f,                   0.1f, 0.9f, 0.1f},

        //mismas pildoras, distancia cercana,ataca, pildora lejana=escapa
            {0, 0, 1, 0,                            0.1f, 0.1f, 0.9f}

    };

    public static RedNeuronalNaves instance;

    void Start()
    {

        instance = this;
        red = new RedNeuronal(numInput, numHidden, numOutput);
        EntrenarRed();
    }

    //funcion que entrena a la red con la matriz de entrenamiento
    //si clicamos el bool randomLearn en el inspector, la matriz de entrenamiento se iniciara con valores aleatorios
    private void EntrenarRed()
    {
        float error = 1;
        int epoch = 0;

        if (randomLearn)
        {
            for (int i = 0; i < entrenamiento.GetLength(0); i++)
            {
                for (int j = 0; j < entrenamiento.GetLength(1); j++)
                {
                    entrenamiento[i, j] = Random.Range(0, 0.99f);
                }
            }
        }

        while ((error > 0.05f) && (epoch < 50000))
        {
            error = 0;
            epoch++;
            for (int i = 0; i < entrenamiento.GetLength(0); i++)
            {
                for (int j = 0; j < numInput; j++)
                    red.SetInput(j, entrenamiento[i, j]);
                for (int j = numInput; j < numInput + numOutput; j++)
                    red.SetOutputDeseado(j - numInput, entrenamiento[i, j]);
                red.FeedForward();
                error += red.CalcularError();
                red.BackPropagation();
            }
            error /= entrenamiento.GetLength(0);
            // VER COMO EVOLUCIONA EL ERROR A MEDIDA QUE AVANZAN LOS EPOCHS
        }


    }
    //Funcion que reentrena la red neuronal
    public void ReentrenarRed (float[] inputs, float[] output) {
        print("REENTRENANDO");
        float error = 1;
        int epoch = 0;

        while ((error > 0.1f) && (epoch < 5000)) {
            epoch++;
            for (int j = 0; j < numInput; j++)
                red.SetInput(j, inputs[j]);
            for (int j = 0; j < numOutput; j++)
                red.SetOutputDeseado(j, output[j]);
            red.FeedForward();
            error = red.CalcularError();
            red.BackPropagation();
            // VER COMO EVOLUCIONA EL ERROR A MEDIDA QUE AVANZAN LOS EPOCHS
        }
    }

    //  FUNCIÓN DE PARA UN INPUT DADO, OBTENER UN OUTPUT

    public byte ConsultarAccion(float[] inputs)
    {
        for(int i=0;i<inputs.Length;i++)
        {
            red.SetInput(i, inputs[i]);
        }

        red.FeedForward();
        return (byte)red.GetMaxOutputId();
        
       
    }

    public float GetOutput (int i) {
        return red.GetOutput(i);
    }
    public int GetMaxOutputId() {
        return red.GetMaxOutputId();
    }
}
