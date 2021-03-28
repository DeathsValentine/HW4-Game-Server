﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public Transform shootOrigin;
    public CharacterController controller;
    public float gravity = -9.81f;
    public float movementSpeed = 5f;  // /Constants.TICKS_PER_SECOND;
    public float jumpSpeed = 5f;
    public float health;
    public float maxHealth = 100f;

    private bool[] inputs;
    private float yVelocity = 0;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        movementSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        inputs = new bool[4];
    }

    public void FixedUpdate()
    {
        if(health <= 0f)
        {
            return;
        }

        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection);
    }

    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        //transform.position += _moveDirection * movementSpeed;
        _moveDirection *= movementSpeed;

        if(controller.isGrounded)
        {
            yVelocity = 0f;
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        controller.Move(_moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }
    
    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Shoot(Vector3 _viewDirection)
    {
        if(health <= 0f)
        {
            return;
        }
        if(Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, 25f))
        {
            if(_hit.collider.CompareTag("Player"))
            {
                _hit.collider.GetComponent<Player>().TakeDamage(50f);
            }
        }
    } 
    
    public void TakeDamage(float _damage)
    {
        if(health <= 0f)
        {
            return;
        }

        health -= _damage;
        if(health <= 0f)
        {
            health = 0f;
            controller.enabled = false;
        }

        ServerSend.PlayerHealth(this);
    }
}   
