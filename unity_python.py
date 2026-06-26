import cv2
import numpy as np
import socket
import mediapipe as mp

# Network configurations
LISTEN_PORT = 5006
SEND_PORT = 5005
TARGET_IP = "127.0.0.1"

LEARN_RATE = 0.05

class BackgroundLearner:
    def __init__(self):
        self.bg = None
    def update(self, frame, person_mask_bin):
        frame_f = frame.astype(np.float32)
        if self.bg is None:
            self.bg = frame_f.copy()
            return
        background_pixels = ~person_mask_bin
        self.bg[background_pixels] = (self.bg[background_pixels] * (1 - LEARN_RATE) + frame_f[background_pixels] * LEARN_RATE)

def get_person_mask(segmenter, frame):
    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    result = segmenter.process(rgb)
    return np.clip(result.segmentation_mask, 0, 1)

def main():
    # Setup ONLY the sending socket (Python -> Unity)
    send_sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    
    # Initialize MediaPipe
    segmenter = mp.solutions.selfie_segmentation.SelfieSegmentation(model_selection=1)
    bg_learner = BackgroundLearner()
    
    # 0. Open Laptop Webcam (0 is usually the built-in camera)
    cap = cv2.VideoCapture(0)
    if not cap.isOpened():
        print("Error: Could not open laptop webcam.")
        return

    # Set resolution down to keep UDP packets lightweight (under 65KB limit)
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 480)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 360)

    print("Python server running with Laptop Webcam! Streaming to Unity...")

    while True:
        try:
            # 1. Grab frame from laptop webcam
            ret, frame = cap.read()
            if not ret or frame is None:
                continue
                
            # 2. Run Cloaking Pipeline
            soft_mask = get_person_mask(segmenter, frame)
            person_bin = soft_mask > 0.5
            bg_learner.update(frame, person_bin)
            
            if bg_learner.bg is not None:
                blurred_mask = cv2.GaussianBlur(soft_mask, (9, 9), 0)
                mask_3ch = blurred_mask[:, :, np.newaxis]
                blended = frame.astype(np.float32) * (1 - mask_3ch) + bg_learner.bg * mask_3ch
                display = np.clip(blended, 0, 255).astype(np.uint8)
            else:
                display = frame.copy()
            
            # Flip image vertically because Unity textures interpret texture spaces inverted
            display = cv2.flip(display, 0)

            # 3. Compress and stream to Unity (Port 5005)
            _, encoded_img = cv2.imencode('.jpg', display, [int(cv2.IMWRITE_JPEG_QUALITY), 60])
            send_sock.sendto(encoded_img.tobytes(), (TARGET_IP, SEND_PORT))

            # Optional: Show a local python preview window to make sure webcam works
            cv2.imshow("Python Live Webcam Cloaking", cv2.flip(display, 0))
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

        except Exception as e:
            print(f"Streaming error: {e}")

    cap.release()
    cv2.destroyAllWindows()

if __name__ == "__main__":
    main()