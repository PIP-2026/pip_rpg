#!/usr/bin/node
"use strict" ;
export { interactionNpcContext  } ;

const interactionNpcContext = {
  "keys": {
    "session_id": { "of": "/statistics/session", "for": [ "/statistics/input", "/statistics/interaction/npc", "/statistics/time" ] },
    "npc_id": { "of": "/characters/npc", "for": [ "/statistics/interaction/npc" ] }
  },
} ;