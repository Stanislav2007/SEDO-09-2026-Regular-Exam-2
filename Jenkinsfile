
pipeline {
    agent any

    tools {
        dotnet 'dotnet8'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                bat 'dotnet restore Homies.sln'
            }
        }

        stage('Build') {
            steps {
                bat 'dotnet build Homies.sln --no-restore --configuration Release'
            }
        }

        stage('Test') {
            steps {
                bat 'dotnet test Homies.sln --no-build --configuration Release'
            }
        }
    }
}
