# Load the data and take a look at it.
data <- read.csv("C:\\Users\\Gary\\Documents\\Presentations\\Codemash2015\\SuperHero\\LogisticRegression\\Data.csv");
head(data);

# Predict NDVI based on time of day and month of year
logit <- glm(NDVI~TOD+MOY, data=data, family="binomial");

summary(logit);

# "Decode" the coefficients
exp(coef(logit))