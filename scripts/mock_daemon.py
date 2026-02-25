import asyncio
import json
import websockets

async def handler(websocket):
    print("Client connected")
    try:
        # Send an initial render model
        initial_render = {
            "type": "render",
            "agent_state": "idle",
            "keypad": {
                "slots": [
                    {"slot": 0, "label": "Prep PR", "sublabel": "/prep-pr", "armed": False},
                    {"slot": 1, "label": "Break Task", "sublabel": "/break", "armed": True},
                    {"slot": 2, "label": "Run Gates", "sublabel": "/gates", "armed": False},
                    {"slot": 6, "label": "PR", "sublabel": "Gate", "armed": False},
                    {"slot": 7, "label": "Issue", "sublabel": "Gate", "armed": False},
                    {"slot": 8, "label": "Receipt", "sublabel": "Gate", "armed": False},
                ]
            }
        }
        await websocket.send(json.dumps(initial_render))
        
        async for message in websocket:
            print(f"Received from plugin: {message}")
            try:
                data = json.loads(message)
                msg_type = data.get("type")
                if msg_type == "keypad_press":
                    slot = data.get("slot")
                    print(f"Keypad Slot {slot} pressed")
                elif msg_type == "dialpad_button_press":
                    button = data.get("button")
                    print(f"Dialpad Button {button} pressed")
                elif msg_type == "adjustment":
                    kind = data.get("kind")
                    delta = data.get("delta")
                    print(f"Adjustment {kind}: {delta}")
            except Exception as e:
                print(f"Error parsing message: {e}")
            
    except websockets.exceptions.ConnectionClosed:
        print("Client disconnected")

async def main():
    async with websockets.serve(handler, "127.0.0.1", 29381):
        print("Mock Daemon started at ws://127.0.0.1:29381")
        print("Waiting for plugin connection...")
        await asyncio.Future()  # run forever

if __name__ == "__main__":
    asyncio.run(main())
