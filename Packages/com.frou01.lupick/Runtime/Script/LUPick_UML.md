```mermaid
---
title: LUPickUp main loop
---

flowchart TD
	StartLoop[/LateUpdateLoop\] -->
	isInit{is init = true}

	isInit --NO--> EndLoop

	isInit --YES-->
	ispostStartFrag{is postStartFrag = true} --NO-->
	IsObjectReady{is ObjectReady} --YES-->
	postStartFrag[[SetOwnerPlayer]] -->
	EndLoop[\Loop/]


	IsObjectReady --NO-->
	EndLoop

	Init[[Init]] -.-
	isInit

	ispostStartFrag --YES-->
	isPicked{is Picked = true}

	isPicked --NO--> 
	isOwnerOnDrop

	isDropping --NO-->
	EndLoop

	isOwnerOnDrop{isOwner} --NO-->
	isDropping

	isdropInitFlag{is dropInitFlag = true} -.NO.->
	isDropping{is DropFlag = true}
	
	isDropping --YES--> onDropped[[onDropped]] --> EndLoop

	isPicked --YES--> 
	onPicked[[OnPicked]] -->
	EndLoop

	isOwnerOnDrop --YES--> 
	isdropInitFlag --YES--> 
	ondropInit[[ondropInit]] --> EndLoop
```
```mermaid
flowchart TD
	subgraph "OnDrop"
		

		MoveObjectTransformOnDropped[[Move Object Transform<br> on Dropped transform]] -->
		SetdropFlag_True

	end
	subgraph "ondropInit"

		FetchTrackingDataOnDrop[[Fetch TrackingData]]


		FetchTrackingDataOnDrop -->
		MoveObjectTransform_ByTrackingDataOnDrop[[Move Object Transform<br> by TrackingData]] -->
		CalculateOffsetOnPlayerOnDrop[[Calculate Offset<br> On Dropped transform]] -->
		SetdropInitFlag_True -->
		SyncOndropInit((sync))
	end
	
	subgraph "onpickInit"
		
		MoveObjectTransformOnPickup[[Move Object Transform<br> on Parent transform]] -->
		CalculateOffsetOnHand[[Calculate Offset<br> On Hand]]-->			
		CalculateOffsetOnBone[[Calculate Offset<br> On HandBone]]-->
		SyncOnpickInit((sync)) -->
		isVR{is VR} --YES--> 
		SetpickInitFlag_True
		
		
		isVR{is VR}--NO-->
		DeskTopWalkAround[[DeskTopWalkAround]] --> 
		SetpickInitFlag_True
		
		DeskTopWalkAround -."2Sec late".- SyncOnDeskTop((sync))
	end
	
	subgraph "OnPicked"
		FetchBoneDataOnPicked[[Fetch BoneData]] -->
		isOwnerOnPicked{isOwner}

		isOwnerOnPicked --NO-->
		MoveObjectTransform_FromBone[[Move Object Transform<br> by BonePosition]]

		isOwnerOnPicked --YES-->
		FetchTrackingDataOnPicked[[Fetch TrackingData]] -->
		ispickInitFlag{is pickInitFlag = true}

		ispickInitFlag --NO-->
		MoveObjectTransform_ByTrackingDataOnPickup
		
		ispickInitFlag --YES-->
		onpickInit_[[onpickInit]]

		onpickInit_ -->
		MoveObjectTransform_ByTrackingDataOnPickup[[Move Object Transform<br> by TrackingData]]

    end
```
```mermaid
---
title: LUPickUp
---

classDiagram
  
  UdonSharpBehaviour <|-- LUPickupBase
  class UdonSharpBehaviour{
		+OnPickup() void
		+OnDrop() void
		+OnPickupUseDown() void
		+ResetPosition() void
		+OnPlayerLeft() void
		+OnOwnershipTransferred() void
		+OnDeserialization() void
  }
  class LUPickupBase {
		---------------VARIABLE-----------
		[UdonSynced] Vector3 ObjectLocalPos
		[UdonSynced] Quaternion ObjectLocalRot
		[UdonSynced] Vector3 ObjectBoneLocalPos 
		[UdonSynced] Quaternion ObjectBoneLocalRot
		Vector3 Local_ObjectTrackingLocalPos
    	Quaternion Local_ObjectTrackingLocalRot
		[UdonSynced] bool RightHand
		Vector3 HandBonePos
		Quaternion HandBoneRot
		TrackingData trackingData
		VRCPlayerApi prevOwner
    	VRCPlayerApi ownerPlayer
		---------------CONSTANT-----------
    	VRCPlayerApi LocalPlayer
		VRC_Pickup Pickup
		Transform TransformCache
		Vector3 First_Pos [Local]
		Quaternion First_Rot [Local]
		bool isLocal = false [SerializeField]
		---------------FLAGS--------------
		bool init = false
		bool postStartFrag = false
		[UdonSynced] bool pickedFlag = false
		bool pickInitFlag = false
		bool dropInitFlag = false
		bool dropFlag = false

		#onPicked() void
		#onpickInit() void
		#ondropInit() void
		#onDropped() void
		#FetchTrackingData() void
		#MoveObjectByTrackingData() void
		#MoveObjectByBone() void
		#MoveObjectByOnTransformOffset() void
		#CalculateOffsetOnTrackingData() void
		#CalculateOffsetOnBone() void
		#CalculateOffsetOnTransform() void
		+OnPickup() void
		+OnDrop() void
		+OnPickupUseDown() void*
		+ResetPosition() void
		+OnPlayerLeft() void
		+OnOwnershipTransferred() void
		+OnDeserialization() void

  }


```