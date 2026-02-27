#!/usr/bin/node
"use strict" ;
const CUSTODIAN = "ResponseServer";
const LOCALHOST = "127.0.0.1" ; // 192.168.189.59
const LOCALPORT = 4141 ; // 80


/*************************/
/****** BASIC SETUP ******/
/*************************/
import express from 'express';
import { param, body, validationResult } from 'express-validator' ;
import http from 'http' ;

var app = express();

var server = http.createServer(app);

app.use( express.json() ) ;


/*************************/
/********* MYSQL *********/
/*************************/

import { createConnection } from 'mysql2/promise';
import { asyncMiddleware } from './util/asyncMiddleware.js';
import { sessionContext } from './context/ctx_session.js';
import { inputContext } from './context/ctx_input.js';
import { timeContext } from './context/ctx_time.js';

let sql_connection = await createConnection({
  host: "localhost",
  user: "root",
  password: "",
  database: "pip_rpg",
  namedPlaceholders: true,
});

sql_connection.connect(function(err) {
  if (err) throw err;
  console.log("Connected!");
});


/*************************/
/******* RESPONSES *******/
/*************************/

import { responseBodyGET } from './format/getf.js';
import { responseBodyPOST } from './format/postf.js';
import { responseBodyPUT } from './format/putf.js';
import { responseBodyDELETE } from './format/deletef.js';


/*************************/
/******** SESSION ********/
/*************************/


app.get( "/statistics/session/all",
  body('custody_chain').notEmpty().isArray().optional(),
  body('cache').isISO8601().toDate().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyGET ;
  responseBody.context = sessionContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "SELECT * FROM session ORDER BY id ASC" ;
  let [ rows, fields ] = await sql_connection.execute( sql ) ;

  if( rows.length == 0 )
  {
    responseBody.ok = false ;
    responseBody.error = "No matching data found.";
    response.json( responseBody ) ;
    return ;
  }

  responseBody.data = [ ] ;

  rows.forEach( row => {
    responseBody.data.push( {
      "id":           row[fields[0].name],
      "started_at":   row[fields[1].name],
      "ended_at":     row[fields[2].name],
      "recorded_at":  row[fields[3].name],
    } ) ;
  } ) ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.get( "/statistics/session/:id",
  param('id').notEmpty().isInt( { min:0 } ).toInt(),
  body('cache').isISO8601().toDate().optional(),
  body('custody_chain').notEmpty().isArray().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyGET ;
  responseBody.context = sessionContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "SELECT * FROM session WHERE id = ?" ;
  let [ rows, fields ] = await sql_connection.execute( sql, [ request.params.id ] ) ;
  /**
    * TODO: error handling
    * ERR: rows contains no elements
    */
  if( rows.length == 0 )
  {
    responseBody.ok = false ;
    responseBody.error = "No matching data found.";
    response.json( responseBody ) ;
    return ;
  }
  responseBody.data = [ {
    "id":           rows[0][fields[0].name],
    "started_at":   rows[0][fields[1].name],
    "ended_at":     rows[0][fields[2].name],
    "recorded_at":  rows[0][fields[3].name],
  } ] ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.post( "/statistics/session",
  body('started_at').isISO8601().toDate(),
  body('ended_at').isISO8601().toDate(),
  body('custody_chain').isArray().notEmpty().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyPOST ;
  responseBody.context = sessionContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "INSERT INTO session (started_at,ended_at) VALUES (?,?)" ;
  let [ okPacket, _ ] = await sql_connection.execute( sql, [ request.body.started_at, request.body.ended_at ] ) ;
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.put( "/statistics/session/:id",
  param('id').notEmpty().isInt( { min:0 } ),
  body('started_at').isISO8601().toDate(),
  body('ended_at').isISO8601().toDate(),
  body('cache').isISO8601().toDate().optional(),
  body('custody_chain').isArray().notEmpty().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyPUT ;
  responseBody.context = sessionContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */
  

  /** Maintain Custody Chain */
  if( Object.hasOwn(request, 'body') && Object.hasOwn(request.body, 'custody_chain') ) {
    responseBody.custody_chain = request.body.custody_chain ;
  }
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "UPDATE session SET started_at = ?, ended_at = ?, recorded_at = current_timestamp() WHERE id = ?" ;
  let [ okPacket, _ ] = await sql_connection.execute( sql, [ request.body.started_at, request.body.ended_at, request.params.id ] ) ;
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.delete( "/statistics/session/:id",
  param('id').notEmpty().isInt( { min:0 } ),
  body('cache').isISO8601().toDate().optional(),
  body('custody_chain').isArray().notEmpty().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyDELETE ;
  responseBody.context = sessionContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "DELETE FROM session WHERE id = ?" ;
  let [ okPacket, _ ] = await sql_connection.execute( sql, request.params.id ) ;
  responseBody.deletions.push( { "location": "/statistics/session", "count": okPacket.affectedRows } ) ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;


/*************************/
/********* INPUT *********/
/*************************/

app.get( "/statistics/session/all/input",
  body('custody_chain').notEmpty().isArray().optional(),
  body('cache').isISO8601().toDate().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyGET ;
  responseBody.context = inputContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "SELECT * FROM input ORDER BY session_id ASC" ;
  let [ rows, fields ] = await sql_connection.execute( sql ) ;
  /**
    * TODO: error handling
    * ERR: rows contains no elements
    */
  if( rows.length == 0 )
  {
    responseBody.ok = false ;
    responseBody.error = "No matching data found.";
    response.json( responseBody ) ;
    return ;
  }
  responseBody.data = [ ] ;

  rows.forEach( row => {
    responseBody.data.push( {
      "session_id":           row[fields[0].name],
      "times_buttons_clicked":row[fields[1].name],
      "distance_moved":       row[fields[2].name],
      "etc":                  row[fields[3].name],
      "recorded_at":          row[fields[4].name],
    } )
  } ) ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.get( "/statistics/session/:session_id/input",
  param('session_id').notEmpty().isInt( { min:0 } ),
  body('custody_chain').notEmpty().isArray().optional(),
  body('cache').isISO8601().toDate().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyGET ;
  responseBody.context = inputContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "SELECT * FROM input WHERE session_id = ?" ;
  let [ rows, fields ] = await sql_connection.execute( sql, [ request.params.session_id ] ) ;
  /**
    * TODO: error handling
    * ERR: rows contains no elements
    */
  if( rows.length == 0 )
  {
    responseBody.ok = false ;
    responseBody.error = "No matching data found.";
    response.json( responseBody ) ;
    return ;
  }
  
  responseBody.data = [ {
    "session_id":           rows[0][fields[0].name],
    "times_buttons_clicked":rows[0][fields[1].name],
    "distance_moved":       rows[0][fields[2].name],
    "etc":                  rows[0][fields[3].name],
    "recorded_at":          rows[0][fields[4].name],
  } ] ;
  /** End */

  
  response.json( responseBody ) ;
} ) ) ;

app.post( "/statistics/session/:session_id/input",
  param('session_id').notEmpty().isInt( { min:0 } ),
  body('times_buttons_clicked').notEmpty().isInt( { min:0 } ),
  body('distance_moved').notEmpty().isInt( { min:0 } ),
  body('etc').notEmpty().isInt( { min:0 } ),
  body('custody_chain').notEmpty().isArray().optional(),
  body('cache').isISO8601().toDate().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyPOST ;
  responseBody.context = inputContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "INSERT INTO input (session_id,times_buttons_clicked,distance_moved,etc) VALUES (?,?,?,?)" ;
  let [ okPacket, _ ] = await sql_connection.execute( sql, [ request.params.id, request.body.times_buttons_clicked, request.body.distance_moved, request.body.etc ] ) ;
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.put( "/statistics/session/:session_id/input",
  param('session_id').notEmpty().isInt( { min:0 } ),
  body('times_buttons_clicked').notEmpty().isInt( { min:0 } ),
  body('distance_moved').notEmpty().isInt( { min:0 } ),
  body('etc').notEmpty().isInt( { min:0 } ),
  body('custody_chain').notEmpty().isArray().optional(),
  body('cache').isISO8601().toDate().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyPUT ;
  responseBody.context = inputContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "UPDATE input SET times_buttons_clicked = ?, distance_moved = ?, etc = ?, recorded_at = current_timestamp() WHERE session_id = ?" ;
  let [ okPacket, _ ] = await sql_connection.execute( sql, [ request.body.times_buttons_clicked, request.body.distance_moved, request.body.etc, request.params.session_id ] ) ;
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.delete( "/statistics/session/:session_id/input",
  param('session_id').notEmpty().isInt( { min:0 } ),
  body('custody_chain').notEmpty().isArray().optional(),
  body('cache').isISO8601().toDate().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyDELETE ;
  responseBody.deletions = [] ;
  responseBody.context = inputContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "DELETE FROM input WHERE id = ?" ;
  let [ okPacket, _ ] = await sql_connection.execute( sql, [ request.params.session_id ] ) ;
  responseBody.deletions.push( { "location": "/statistics/session/input", "count": okPacket.affectedRows } ) ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;


/*************************/
/********* TIME **********/
/*************************/

app.get( "/statistics/session/all/time",
  body('custody_chain').notEmpty().isArray().optional(),
  body('cache').isISO8601().toDate().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyGET ;
  responseBody.context = timeContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "SELECT * FROM time ORDER BY session_id ASC" ;
  let [ rows, fields ] = await sql_connection.execute( sql ) ;
  /**
    * TODO: error handling
    * ERR: rows contains no elements
    */
  if( rows.length == 0 )
  {
    responseBody.ok = false ;
    responseBody.error = "No matching data found.";
    response.json( responseBody ) ;
    return ;
  }
  responseBody.data = [ ] ;

  rows.forEach( row => {
    responseBody.data.push( {
      "session_id":     row[fields[0].name],
      "in_menus":       row[fields[1].name],
      "in_exploration": row[fields[2].name],
      "in_dialogue":    row[fields[3].name],
      "recorded_at":    row[fields[4].name],
    } )
  } ) ;
  /** End */
  

  response.json( responseBody ) ;
} ) ) ;

app.get( "/statistics/session/:session_id/time",
  param('session_id').notEmpty().isInt( { min:0 } ),
  body('custody_chain').notEmpty().isArray().optional(),
  body('cache').isISO8601().toDate().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyGET ;
  responseBody.context = timeContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "SELECT * FROM time WHERE session_id = ?" ;
  let [ rows, fields ] = await sql_connection.execute( sql, [ request.params.session_id ] ) ;
  /**
    * TODO: error handling
    * ERR: rows contains no elements
    */
  if( rows.length == 0 )
  {
    responseBody.ok = false ;
    responseBody.error = "No matching data found.";
    response.json( responseBody ) ;
    return ;
  }

  responseBody.data = [ {
    "session_id":     rows[0][fields[0].name],
    "in_menus":       rows[0][fields[1].name],
    "in_exploration": rows[0][fields[2].name],
    "in_dialogue":    rows[0][fields[3].name],
    "recorded_at":    rows[0][fields[4].name],
  } ] ;
  /** End */
  

  response.json( responseBody ) ;
} ) ) ;

app.post( "/statistics/session/:session_id/time",
  param('session_id').notEmpty().isInt( { min:0 } ),
  body('in_menus').notEmpty().isFloat(),
  body('in_exploration').notEmpty().isFloat(),
  body('in_dialogue').notEmpty().isFloat(),
  body('custody_chain').notEmpty().isArray().optional(),
  body('cache').isISO8601().toDate().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyPOST ;
  responseBody.context = timeContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "INSERT INTO time (session_id,in_menus,in_exploration,in_dialogue) VALUES (?,?,?,?)" ;
  let [ okPacket, _ ] = await sql_connection.execute( sql, [ request.params.session_id, request.body.in_menus, request.body.in_exploration, request.body.in_dialogue ] ) ;
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.put( "/statistics/session/:session_id/time",
  param('session_id').notEmpty().isInt( { min:0 } ),
  body('in_menus').notEmpty().isFloat(),
  body('in_exploration').notEmpty().isFloat(),
  body('in_dialogue').notEmpty().isFloat(),
  body('custody_chain').notEmpty().isArray().optional(),
  body('cache').isISO8601().toDate().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyPUT ;
  responseBody.context = timeContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "UPDATE time SET in_menus = ?, in_exploration = ?, in_dialogue = ?, recorded_at = current_timestamp() WHERE session_id = ?" ;
  let [ okPacket, _ ] = await sql_connection.execute( sql, [ request.body.in_menus, request.body.in_exploration, request.body.in_dialogue, request.params.session_id ] ) ;
  /**
    * TODO: error handling
    */
  responseBody.insert_id = okPacket.insertId ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

app.delete( "/statistics/session/:session_id/time",
  param('session_id').notEmpty().isInt( { min:0 } ),
  body('custody_chain').notEmpty().isArray().optional(),
  body('cache').isISO8601().toDate().optional(),
  asyncMiddleware( async (request,response,next) => {
  /** Query Validation */
  const result = validationResult(request) ;
  if ( ! result.isEmpty() )
  {
    return response.send( { errors: result.array() } ) ;
  }
  /** End */


  /** Create Body + Add Context + Maintain Custody Chain */
  let responseBody = responseBodyDELETE ;
  responseBody.deletions = [] ;
  responseBody.context = timeContext ;
  if( Object.hasOwn( request.body, 'custody_chain' ) )
    responseBody.custody_chain = request.body.custody_chain ;
  responseBody.custody_chain.push( CUSTODIAN ) ;
  /** End */


  /** Query SQL Server */
  const sql = "DELETE FROM time WHERE id = ?" ;
  let [ okPacket, _ ] = await sql_connection.execute( sql, [ request.params.session_id ] ) ;
  responseBody.deletions.push( { "location": "/statistics/session/time", "count": okPacket.affectedRows } ) ;
  /** End */
  
  response.json( responseBody ) ;
} ) ) ;

/*************************/
/********* START *********/
/*************************/

server.listen(LOCALPORT,LOCALHOST);

console.log(`Server listening on: http://${LOCALHOST}:${LOCALPORT}`);