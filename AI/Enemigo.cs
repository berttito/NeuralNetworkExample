using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemigo : MonoBehaviour {


    private Rigidbody2D rb;
    private SpriteRenderer sr1;
    private SpriteRenderer sr2;
    public float vel = 3;
    public byte estado = 0;// 0 Recolectar , 1 Atacar, 2 Escapar

    private float vidaMax = 100, vidaAct = 100;
    public List<Enemigo> vecinos;

    public float[] inputs;

    //guarda las balas instanciadas
    List<GameObject> bullets;

    //guarda el prefab de la bala
    [SerializeField]
    GameObject bulletPrefab;

    //lista de pildoras
    public List<Transform> targets;

    //variable que guarda la posicion de la pildora mas cercana
    public Vector3 nearestPill;

    //variables de velocidad del enemigo
    [SerializeField]
    float rotationSpeed;
    [SerializeField]
    float playerSpeed;

    //variable para pintar en el canvas la puntuacion
    [SerializeField]
    Text puntuationPlayer;

    //guarda el numero de pildoras que tiene el enemigo
    float puntuation;

    int direction;

    int activePlayer;
    int inActivePlayer;

    bool isAttacking;

    [SerializeField]
    List<Transform> aircraftList;
    [SerializeField]
    List<Collider2D> colliderList;

    Vector3 distance;

    public bool IsSecondActive { get; set; }

    float height;
    float width;

    [SerializeField]
    float airPlaneSize;

    //variable para utilizada para asignar los outputs e inputs del frame anterior
    float timer = 0;

    //array para guardas los outputs
    float [] outputs=new float [3];
    //array para guardar los inputs del frame anterior
    float[] inputsAnteriores;

    // Use this for initialization
    //inicializamos valores
    void Start () {
        isAttacking = false;

        height = Camera.main.orthographicSize;
        width = (height * Camera.main.aspect);

        activePlayer = 0;
        inActivePlayer = 1;

        rb = GetComponent<Rigidbody2D>();
        sr1 = transform.GetChild(0).GetComponent<SpriteRenderer>();
        sr2 = transform.GetChild(1).GetComponent<SpriteRenderer>();
        vecinos = new List<Enemigo>();
        inputs = new float[4];
        inputsAnteriores = new float[4];

        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = inputsAnteriores[i];
        }
    }
	
	// Update is called once per frame
	void Update () {
        //llama a la funcion decidir estado
        DecidirEstado();

        //dependiendo del estado llama a la funcion de recolectar atacar o escapar
        switch (estado)
        {
            case 0:
                Recolectar();
                break;
            case 1:
                Atacar();
                break;
            case 2:
                Escapar();
                break;
        }

        //llama a la funcion cameralimits
        CameraLimits();
        //Movimiento del objeto enemigo, se contemplan dos enemigos ya que existen dos para cuando uno excede los limites de la pantalla
        
        aircraftList[activePlayer].position += aircraftList[activePlayer].up * Time.deltaTime * playerSpeed;

        if (IsSecondActive)
        {
            aircraftList[inActivePlayer].position += aircraftList[inActivePlayer].up * Time.deltaTime * playerSpeed;
        }
    }

    //Comportamiento de recoleccion de pildoras
    //el enemigo se dirige a la pildora mas cercana detectada por el sensor
    private void Recolectar() 
    {
        if(targets.Count != 0)
        {
            Vector3 vectLocal = aircraftList[activePlayer].InverseTransformDirection(nearestPill - aircraftList[activePlayer].position);

            aircraftList[activePlayer].Rotate(0, 0, -Mathf.Sign(vectLocal.x) * rotationSpeed * Time.deltaTime);
            aircraftList[inActivePlayer].rotation = aircraftList[activePlayer].rotation;
        }
    }
    //comportamiento de atacar del enemigo
    //se dirige hacia la posicion del player y cuando esta encarado hacia el le dispara
    private void Atacar() 
    {
        Vector3 vectLocal = aircraftList[activePlayer].InverseTransformDirection(SlitherPlayerController.instance.aircraftList[SlitherPlayerController.instance.activePlayer].position - aircraftList[activePlayer].position);

        aircraftList[activePlayer].Rotate(0, 0, -Mathf.Sign(vectLocal.x) * rotationSpeed * Time.deltaTime);
        aircraftList[inActivePlayer].rotation = aircraftList[activePlayer].rotation;

        if(Mathf.Abs(vectLocal.x) < 0.5 && !isAttacking)
        {
            Shoot();
        }
    }
    //funcion que cambia el valor del bool isAttacking para indicar que el enemigo ya no esta atacando
    void CancelAttack() {
        isAttacking = false;
    }

    //funcion que instancia las balas si el enemigo dispone de ellas
    //se decrementa el numero de balas mostrado por pantalla
    //la bala se destruye si colisiona con el player o si pasa un segundo desde que fue disparada
    private void Shoot() {
        if (puntuation > 0 && !isAttacking) {
            isAttacking = true;
            Invoke("CancelAttack", 1f);

            puntuation--;
            puntuationPlayer.text = "IA: " + puntuation.ToString();

            GameObject bullet = Instantiate(bulletPrefab, aircraftList[activePlayer].position, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().AddForce(aircraftList[activePlayer].up * 10, ForceMode2D.Impulse);

            Destroy(bullet, 1);
        }
    }
    //Comportamiento de escapar
    // cuando el player dispara el enemigo hace fintas para evitar ser alcanzado por la bala
    private void Escapar() 
    {
        // sacar la normal
        float newFloat = SlitherPlayerController.instance.aircraftList[SlitherPlayerController.instance.activePlayer].rotation.eulerAngles.z - aircraftList[activePlayer].rotation.eulerAngles.z + 90;

        //saca distancia jugadores
        Vector3 vectLocal = aircraftList[activePlayer].InverseTransformDirection(aircraftList[activePlayer].position - SlitherPlayerController.instance.aircraftList[SlitherPlayerController.instance.activePlayer].position);

        //si esta cerca huye
        if(Mathf.Abs(vectLocal.x ) < 5)
            aircraftList[activePlayer].Rotate(0, 0,Mathf.Sign(newFloat) * rotationSpeed * Time.deltaTime);

        aircraftList[inActivePlayer].rotation = aircraftList[activePlayer].rotation;   
    }

    //Colorea el sprite del enemigo con el color indicado como parametro
    public void Colorear(Color color)
    {
        sr1.color = color;
        sr2.color = color;
    }

    //Funcion para incrementar la puntuacion cada vez que la nave enemiga recoge una pildora
    public void IncreaseScale() 
    {
        puntuation++;
        puntuationPlayer.text = "IA: " + puntuation.ToString();
    }

    //Funcion que decide el estado en base a los inputs y outputs y el reentrenamiento de la red neuronal
    private void DecidirEstado()
    {
        

        // INPUT: Diferencia contador, rango,  ataque ,distancia al powerUp


        //Diferencia de puntuacion del enemigo y el player dividido entre la max puntuacion para que el rango de valores este comprendido 
        //entre 0 y 1
        inputs[0] = ( puntuation - SlitherPlayerController.instance.puntuation) / Const.MAX_PUNTUACION;
        //Si el input 0 es menor o igual a cero igualamos a cero para que no existan valores negativos
        if(inputs[0]<=0)
        {
            inputs[0] = 0;
        }
        //calcula la distancia que hay entre el enemigo y el player y se divide entre la distancia maxima para que el rango de valores
        //este comprendido entre 0 y 1
        inputs[1] = Vector3.Distance(SlitherPlayerController.instance.aircraftList[SlitherPlayerController.instance.activePlayer].position, aircraftList[activePlayer].position) / Const.MAX_DISTANCIA;

        //Si no detecta pildora cercana se le asigna un 1 al input
        if(GetNearestPill() == new Vector3(9999, 9999))
        {
            inputs[3] = 1;
        }
        //si hay pildoras dentro del rango calcula la distancia y la divide entre la distancia maxima para el rango de valores entre 0 y 1
        else
        {
            inputs[3] = Vector3.Distance(GetNearestPill(), aircraftList[activePlayer].position) / Const.MAX_DISTANCIA_PILDORA;
        }

        //Comprueba si el player esta atacando, si no ataca el input es 0 y si ataca el input es 1
        if (!SlitherPlayerController.instance.isAttcking)
        {
            inputs[2] = 0f;
        }
        else
        {
            inputs[2] = 1f;
        }

        //guarda el estado que debe adoptar dependiendo del resultado de los inputs
        estado = RedNeuronalNaves.instance.ConsultarAccion(inputs);

        //reentrenar red
        //Cuando el timer sea mayor a un segundo pasara y asignara los outputs e inputs del frame anterior a la funcion reentrenar red
        if(timer>1)
        {
            //refuerzo: si el contador del enemigo esta en cero pasara directamente al estado de recoleccion
            if (inputs[0] <= 0)
            {
                float[] outputs = { 0.9f, 0.1f, 0.1f };
                RedNeuronalNaves.instance.ReentrenarRed(inputsAnteriores, outputs);
            }

            

            RedNeuronalNaves.instance.ReentrenarRed(inputsAnteriores, outputs);
            timer = 0;
        }
        //Incrementamos el timer y guardamos los valores de input y output
        //en el siguiente frame estos valores se asignaran en la funcion de reentrenar red
        timer += Time.deltaTime;
        for (int i=0;i<inputs.Length;i++)
        {
            inputs[i] = inputsAnteriores[i];
        }

        outputs[0] = RedNeuronalNaves.instance.GetOutput(0);
        outputs[1] = RedNeuronalNaves.instance.GetOutput(1);
        outputs[2] = RedNeuronalNaves.instance.GetOutput(2);
        ////end

        //dependiendo del estado la nave enemiga adquirira un color diferente mediante la funcion colorear
        {

            switch (estado)
            {
                case 0:
                    Colorear(Color.yellow);
                    break;
                case 1:
                    Colorear(Color.red);
                    break;
                case 2:
                    Colorear(Color.green);
                    break;
            }

        }
            
        

        
        

    }

    
    //funcion que asigna la pildora mas cercana, si no hay pildoras cerca asigna un vector de gran valor para que el input se ponga a 1
    private Vector3 GetNearestPill()
    {
        nearestPill = new Vector2(9999,9999);

        for(int i = 0; i < targets.Count; i++) 
        {
            if(nearestPill == new Vector3(9999, 9999)) 
            {
                nearestPill = targets[i].position;
            }
            else
            {
                nearestPill = (Vector3.Distance(aircraftList[activePlayer].transform.position, targets[i].position)) < (Vector3.Distance(aircraftList[activePlayer].transform.position, targets[i].position)) ? targets[i].position : nearestPill;
            }

            return nearestPill;
        }

        return nearestPill;
    }

    //Metodo que activa el avion enemigo primero o segundo, si uno de estos sale de la pantalla
    //el otro se activara y entrara por el lado contrario de la pantalla
    public int ChangeAircraft() {
        switch (activePlayer) {
            case 0:
                activePlayer = 1;
                inActivePlayer = 0;
                return activePlayer;
            case 1:
                activePlayer = 0;
                inActivePlayer = 1;
                return activePlayer;
        }

        return 0;
    }

    //establece los limites de la pantalla y dice donde tiene que colocar los aviones y su rotacion
    private void CameraLimits() {
        #region check if player leaves window
        if (aircraftList[activePlayer].position.x >= width) {
            inActivePlayer = activePlayer;

            activePlayer = ChangeAircraft();

            colliderList[activePlayer].gameObject.SetActive(true);

            IsSecondActive = true;
            aircraftList[activePlayer].position = new Vector3(-width, aircraftList[inActivePlayer].position.y, 0);
        }
        else if (aircraftList[activePlayer].position.x <= -width) {
            inActivePlayer = activePlayer;

            activePlayer = ChangeAircraft();

            colliderList[activePlayer].gameObject.SetActive(true);

            IsSecondActive = true;
            aircraftList[activePlayer].position = new Vector3(width, aircraftList[inActivePlayer].position.y, 0);
        }
        else if (aircraftList[activePlayer].position.y >= height) {
            inActivePlayer = activePlayer;

            activePlayer = ChangeAircraft();


            colliderList[activePlayer].gameObject.SetActive(true);

            IsSecondActive = true;
            aircraftList[activePlayer].position = new Vector3(aircraftList[inActivePlayer].position.x, -height, 0);
        }
        else if (aircraftList[activePlayer].position.y <= -height) {
            inActivePlayer = activePlayer;

            activePlayer = ChangeAircraft();

            colliderList[activePlayer].gameObject.SetActive(true);

            IsSecondActive = true;
            aircraftList[activePlayer].position = new Vector3(aircraftList[inActivePlayer].position.x, height, 0);
        }
        #endregion


        #region stop the player out of window
        if (aircraftList[inActivePlayer].position.x >= width + airPlaneSize) {

            colliderList[inActivePlayer].gameObject.SetActive(false);
            IsSecondActive = false;
        }
        else if (aircraftList[inActivePlayer].position.x <= -width - airPlaneSize) {

            colliderList[inActivePlayer].gameObject.SetActive(false);
            IsSecondActive = false;
        }
        else if (aircraftList[inActivePlayer].position.y >= height + airPlaneSize) {

            colliderList[inActivePlayer].gameObject.SetActive(false);
            IsSecondActive = false;
        }
        else if (aircraftList[inActivePlayer].position.y <= -height - airPlaneSize) {

            colliderList[inActivePlayer].gameObject.SetActive(false);
            IsSecondActive = false;
        }
        #endregion
    }
}
