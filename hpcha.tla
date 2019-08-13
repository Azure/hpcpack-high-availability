------------------------------- MODULE hpcha -------------------------------
EXTENDS Integers, FiniteSets

CONSTANT SERVER

VARIABLE serverState
VARIABLE currentLeader
VARIABLE serverHeartBeat

vars == <<serverState, currentLeader, serverHeartBeat>>

TypeOK == 
  /\ serverState \in [SERVER -> {"follower", "leader"}]
  /\ serverHeartBeat \in [SERVER -> {"heartbeating", "stopped"}]
  /\ currentLeader \subseteq SERVER
  
Init ==
  /\ serverState = [s \in SERVER |-> "follower"]
  /\ serverHeartBeat = [s \in SERVER |-> "stopped"]
  /\ currentLeader = {}
  
StartHeartBeat(s) ==
  /\ \/ currentLeader = {}
     \/ serverState[s] = "leader"
  /\ serverHeartBeat' = [serverHeartBeat EXCEPT ![s] = "heartbeating"]
  /\ UNCHANGED <<currentLeader, serverState>>
  
StopHeartBeat(s) ==
  /\ currentLeader # {}
  /\ s \notin currentLeader 
  /\ serverHeartBeat' = [serverHeartBeat EXCEPT ![s] = "stopped"]
  /\ UNCHANGED <<currentLeader, serverState>>
  
(***************************************************************************)
(* ServerCrash should always trigger LeaderLost to ensure safety.  This    *)
(* can be guaranteed in implementation by generate new server ID on        *)
(* restart.                                                                *)
(***************************************************************************)
ServerCrach(s) ==
  /\ serverState' = [serverState EXCEPT ![s] = "follower"]
  /\ serverHeartBeat' = [serverHeartBeat EXCEPT ![s] = "stopped"]
  /\ currentLeader' = currentLeader \ {s}
  
StepUp(s) ==
  /\ serverState[s] = "follower"
  /\ s \in currentLeader
  /\ serverState' = [serverState EXCEPT ![s] = "leader"]
  /\ UNCHANGED <<currentLeader, serverHeartBeat>>  

LeaderLost ==
  /\ currentLeader # {}
  /\ \E s \in currentLeader: 
     /\ serverHeartBeat[s] = "stopped"
     /\ currentLeader' = currentLeader \ {s}
  /\ UNCHANGED <<serverState, serverHeartBeat>>
  
LeaderElected ==
  /\ currentLeader = {}
  /\ \E s \in SERVER:
      /\ serverHeartBeat[s] = "heartbeating"
      /\ currentLeader' = currentLeader \cup {s}
  /\ UNCHANGED <<serverHeartBeat, serverState>>
  
Next == 
  \/ LeaderElected \/ LeaderLost
  \/ \E s \in SERVER: \/ ServerCrach(s)
                      \/ StartHeartBeat(s)
                      \/ StopHeartBeat(s)
                      \/ StepUp(s)
                    
SingleLeaderElected == Cardinality(currentLeader) <= 1   
SingleLeaderStepUp == \/ \E s \in SERVER: /\ serverState[s] = "leader"
                                          /\ \A s2 \in SERVER \ {s}: serverState[s2] = "follower"
                      \/ \A s \in SERVER: serverState[s] = "follower"
                      

Spec == Init /\ [][Next]_vars
FairSpec == /\ Spec 
            /\ SF_vars(LeaderElected) /\ WF_vars(LeaderLost) 
            /\ \A s \in SERVER: /\ SF_vars(StepUp(s)) 
                                /\ WF_vars(StartHeartBeat(s))
                                /\ WF_vars(StopHeartBeat(s))
                        
EventualHeartBeat == currentLeader = {} ~> (\E s \in SERVER: serverHeartBeat[s] = "heartbeating")
EventualElected == (\E s \in SERVER: serverHeartBeat[s] = "heartbeating") ~> currentLeader # {}
EventualStepUp == currentLeader # {} ~> (\E s \in SERVER: serverState[s] = "leader")
=============================================================================
\* Modification History
\* Last modified Mon Jun 17 10:22:28 CST 2019 by zihche
\* Created Wed Jun 12 18:37:17 CST 2019 by zihche
