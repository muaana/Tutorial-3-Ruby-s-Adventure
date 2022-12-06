using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RubyController : MonoBehaviour
{
    //Variables
    Rigidbody2D rigidbody2d;
    float horizontal; 
    float vertical;
    public float speed = 4.0f;
    public static int level = 1;

    //Projectile
    public GameObject projectilePrefab;
    private int cogCount = 5;

    //Health
    public int maxHealth = 10;
    public int health { get { return currentHealth; }}
    int currentHealth;

    //Speed Boosts
    public float timeBoosting = 4.0f;
    float speedBoostTimer;
    bool isBoosting;

    //Invincibility
    public float timeInvincible = 2.0f;
    bool isInvincible;
    float invincibleTimer;

    //Animator
    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);

    //Audio
    AudioSource audioSource;
    public AudioSource background;
    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip coinPickup;

    //Particles
    public ParticleSystem healthDecrease;
    public ParticleSystem healthIncrease;

    //Win/Lose Text
    public TextMeshProUGUI fixedText;
    public TextMeshProUGUI cogText;
    int scoreFixed = 0;
    public TextMeshProUGUI coinText;
    int coinCount = 0;
    public GameObject winText;
    public GameObject loseText;
    private bool gameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        //Animator/Rigidbody
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        //Health
        currentHealth = maxHealth;

        //Audio
        audioSource = GetComponent<AudioSource>();
        background.Play();

        //Fixed Robot Text
        fixedText.text = "Fixed Robots: " + scoreFixed.ToString() + "/5";

        //cogs text
        cogText.text = "Cogs: " + cogCount;

        //Win/Lose Text
        winText.SetActive(false);
        loseText.SetActive(false);
        gameOver = false;

        scoreFixed = 0;
        SetFixedText();

        level = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //speedboost
        if (isBoosting == true)
        {
            speedBoostTimer -= Time.deltaTime; // Once speed boost activates, it counts down
            speed = 8;
            healthIncrease.Play();
        
            if (speedBoostTimer < 0)
            {
                isBoosting = false;
                speed = 5; 
            }
        }

        //Movement
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        //Anim/Flip
        Vector2 move = new Vector2(horizontal, vertical);

        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }
        
        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        //Invincible Timer
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        //Cog Launch
        if(Input.GetKeyDown(KeyCode.C))
        {
            Launch();
        }

        //Npc dialouge
        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));

            if (hit.collider != null)
            {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                
                if (character != null)
                {
                    character.DisplayDialog();
                }

                if (scoreFixed == 5)
                {
                    SceneManager.LoadScene("Level2");
                    level = 2;
                }

                if (character != null && level ==2)
                {
                    character.DisplayDialog();
                }
            }
        }

        //close game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        //Restart
        if (Input.GetKey(KeyCode.R))
        {

            if (gameOver == true && level == 2)

            {
              SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (gameOver == true && level == 1)
            {
                SceneManager.LoadScene("Level1");
            }
        }
    }

    void FixedUpdate()
    {
        //Movement + Speed Value
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }

    //health
    public void ChangeHealth(int amount)
    {
        if (amount > 0)
        {
            healthIncrease.Play();
            ParticleSystem projectilePrefab = Instantiate(healthIncrease, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        }

        if (currentHealth <= 0)
        {
            gameOver = true;
            speed = 0;
        
            loseText.SetActive(true);

            //Audio
            background.Stop();
            PlaySound(loseSound);

            Destroy(gameObject.GetComponent<SpriteRenderer>());
            //healthDecrease.Pause();   must fix later
        }

        //Invincible
        if (amount < 0)
        {
            animator.SetTrigger("Hit");
            
            if (isInvincible)
                return;

                isInvincible = true;
                invincibleTimer = timeInvincible;

                //Health Decrease Particle
                healthDecrease.Play();
                ParticleSystem projectilePrefab = Instantiate(healthDecrease, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

                PlaySound(hitSound);  
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
    }

    //Projectile
    void Launch()
    {
        if (cogCount > 0)
        {
            GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            projectile.Launch(lookDirection, 300);

            animator.SetTrigger("Launch");

            PlaySound(throwSound);

            cogCount -= 1;
            SetCogText();
        }
    }
    
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    void SetFixedText()
    {
        fixedText.text = "Fixed Robots: " + scoreFixed.ToString() + "/5";
    }

    void SetCogText()
    {
        cogText.text = "Cogs: " + cogCount.ToString(); 
    }

    void SetCoinText()
    {
        coinText.text = "Coin: " + coinCount.ToString() + "/7";
    }
    //coin meter
    public void CoinAmounts(int amount)
    {
        if (level == 2)
        {
            coinCount = 0;
            SetCoinText();
            coinCount += amount;
            coinText.text = "Coin: " + coinCount.ToString() + "/7";
        }
    }

    public void FixedRobots(int amount)
    {
        scoreFixed += amount;
        fixedText.text = "Fixed Robots: " + scoreFixed.ToString() + "/5";

        Debug.Log("Fixed Robots: " + scoreFixed);

        if (scoreFixed == 5 && level == 1)
        {
            winText.SetActive(true);
        }

        if (scoreFixed == 5 && coinCount == 7)
        {
            winText.SetActive(true);
            loseText.SetActive(false);
            speed = 0;
            background.Stop();
            PlaySound(winSound);
            gameOver = true;
            level = 2;
        } 

        //Debug.Log(if (level == 2 && coinCount >= 7));
    }

   

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ammo"))
        {
            cogCount += 5;
            SetCogText();
            other.gameObject.SetActive(false);
            audioSource.Play();
        }

        else if (other.gameObject.CompareTag("Coin"))
        {
            coinCount += 1;
            SetCoinText();
            other.gameObject.SetActive(false);
            PlaySound(coinPickup);
        }        
    }

    public void SpeedBoost(int amount)
    {
        if (amount > 0)
        {
            speedBoostTimer = timeBoosting;
            isBoosting = true;
        }
    }
}