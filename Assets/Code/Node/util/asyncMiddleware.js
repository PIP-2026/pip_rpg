#!/usr/bin/node
"use strict" ;
export { asyncMiddleware };

const asyncMiddleware = fn => ( request, response, next ) => { Promise.resolve(fn(request,response,next)).catch(next) ; } ;