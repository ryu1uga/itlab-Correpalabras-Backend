import subprocess

def main():
    subprocess.run([
        "docker", "compose",
        "-f", "docker-compose.yml",
        "up",
        "-d", "--build"
    ])

if __name__ == "__main__":
    main()