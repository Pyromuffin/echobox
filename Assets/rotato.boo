import UnityEngine

class rotato (MonoBehaviour): 

	def Start ():
		pass
	
	def Update ():
		transform.Rotate(Vector3(0,50 * Time.deltaTime,30 * Time.deltaTime))
		