using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMover : MonoBehaviour
{
    public float speed;
    public bool vertical;
    public float changeTime = 3.0f;

    public float timeDecreasing = 3.0f;
    float speedDecreaseTimer;
    
    //Rigidboyd etc
    Rigidbody2D rigidbody2D;
    float timer;
    int direction = 1;
    
    //Anim
    Animator animator;

    //Audio
    AudioSource audioSource;

    //Ruby Controller
    private RubyController rubyController;
    
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        timer = changeTime;

        //anim
        animator = GetComponent<Animator>();

        //Audio
        audioSource = GetComponent<AudioSource>();

        //Ruby
        GameObject rubyControllerObject = GameObject.FindWithTag("RubyController");
        rubyController = rubyControllerObject.GetComponent<RubyController>();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0)
        {
            direction = -direction;
            timer = changeTime;
        }
    }
    
    void FixedUpdate()
    {

        Vector2 position = rigidbody2D.position;
        
        if (vertical)
        {
            position.y = position.y + Time.deltaTime * speed * direction;
            animator.SetFloat("Move X", 0);
            animator.SetFloat("Move Y", direction);
        }
        else
        {
            position.x = position.x + Time.deltaTime * speed * direction;
            animator.SetFloat("Move X", direction);
            animator.SetFloat("Move Y", 0);
        }
        
        rigidbody2D.MovePosition(position);

    }
    
    //damage
    void OnCollisionEnter2D(Collision2D other)
    {
        RubyController player = other.gameObject.GetComponent<RubyController >();

        if (player != null)
        {
            player.ChangeHealth(-5);
            speedDecreaseTimer -= Time.deltaTime;
            player.speed = 1;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
